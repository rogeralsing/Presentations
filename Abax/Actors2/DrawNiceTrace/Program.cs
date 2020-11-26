﻿using Jaeger;
using Jaeger.Samplers;
using OpenTracing.Util;
using Proto;
using Proto.OpenTracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrawNiceTrace
{
    class Program
    {
        static async Task Main()
        {
            var serviceName = $"DrawNiceTrace{Guid.NewGuid()}";

            var tracer = new Tracer.Builder(serviceName)
                .WithSampler(new ConstSampler(true))
                .Build();
            GlobalTracer.Register(tracer);
            var system = new ActorSystem();
            var rootContext = new RootContext(system, new MessageHeader(), OpenTracingExtensions.OpenTracingSenderMiddleware())
                .WithOpenTracing();

            var bankProps = Props.FromFunc(
                    ctx =>
                    {
                        if (ctx.Message is ProcessPayment _) ctx.Respond(new ProcessPaymentResponse { Ok = true });
                        return Task.CompletedTask;
                    }
                )
                .WithOpenTracing();
            var bank = rootContext.Spawn(bankProps);

            var restaurantProps = Props.FromFunc(
                    async ctx =>
                    {
                        if (ctx.Message is TriggerFood trigger)
                        {
                            using (GlobalTracer.Instance.BuildSpan("Preparing food !").StartActive())
                            {
                                await Task.Delay(1000);
                            }

                            ctx.Send(trigger.Customer, new DeliverFood());
                        }
                    }
                )
                .WithOpenTracing();
            var restaurant = rootContext.Spawn(restaurantProps);

            var cartProps = Props.FromProducer(() => new CartActor(bank)).WithOpenTracing();
            long nextCartNumber = 1;

            string getActorName(long number) => $"cart-{number}";

            var dinnerProps = Props.FromFunc(
                    async ctx =>
                    {
                        switch (ctx.Message)
                        {
                            case AddItem addItem:
                                PID cartPID;

                                if (addItem.CartNumber == default)
                                {
                                    var cartNumber = nextCartNumber++;
                                    cartPID = ctx.SpawnNamed(cartProps, getActorName(cartNumber));
                                    ctx.Send(cartPID, new AssignCartNumber { CartNumber = cartNumber });
                                }
                                else
                                {
                                    var cartActorName = getActorName(addItem.CartNumber);
                                    cartPID = ctx.Children.First(p => p.Id.EndsWith(cartActorName));
                                }

                                ctx.Forward(cartPID);
                                break;

                            case ConfirmOrder confirmOrder:
                                var orderPid = ctx.Children.First(p => p.Id.EndsWith(getActorName(confirmOrder.CartNumber)));
                                ctx.Forward(orderPid);
                                break;

                            case SendPaymentDetails sendPaymentDetails:
                                var orderPid2 = ctx.Children.First(p => p.Id.EndsWith(getActorName(sendPaymentDetails.OrderNumber)));
                                var resp = await ctx.RequestAsync<ProcessPaymentResponse>(orderPid2, sendPaymentDetails);

                                if (resp.Ok) // it will always, we have super rich customers
                                    ctx.Send(
                                        restaurant,
                                        new TriggerFood { Customer = sendPaymentDetails.Customer, OrderNumber = sendPaymentDetails.OrderNumber }
                                    );
                                break;
                        }
                    }
                )
                .WithOpenTracing();
            var dinner = rootContext.Spawn(dinnerProps);

            var customerProps = Props.FromFunc(
                    async ctx =>
                    {
                        switch (ctx.Message)
                        {
                            case Started _:
                                var cartNumberResponse = await ctx.RequestAsync<CartNumberResponse>(dinner, new AddItem { ProductName = "Beer!" });
                                var cartNumber = cartNumberResponse.CartNumber;

                                await Task.Delay(100);
                                await ctx.RequestAsync<CartNumberResponse>(dinner, new AddItem { CartNumber = cartNumber, ProductName = "Pizza." });
                                await Task.Delay(100);
                                await ctx.RequestAsync<CartNumberResponse>(dinner, new AddItem { CartNumber = cartNumber, ProductName = "Ice cream." });

                                await Task.Delay(200);
                                var confirmed = await ctx.RequestAsync<ConfirmOrderResponse>(dinner, new ConfirmOrder { CartNumber = cartNumber });

                                await Task.Delay(300);
                                ctx.Send(dinner, new SendPaymentDetails { Customer = ctx.Self, OrderNumber = confirmed.OrderNumber });

                                break;

                            case DeliverFood deliver:
                                throw new Exception("Food Delivered !");
                        }
                    }
                )
                .WithOpenTracing()
                .WithGuardianSupervisorStrategy(new OneForOneStrategy((pid, reason) => SupervisorDirective.Stop, 0, null));

            rootContext.Spawn(customerProps);

            await Task.Delay(TimeSpan.FromSeconds(10));
            Console.WriteLine("Done! Go to jaeger : http://localhost:16686/search?service=" + serviceName);
            Console.WriteLine("Press [Enter]");
            Console.ReadLine();
        }

        class CartActor : IActor
        {
            private readonly PID _bank;

            private readonly List<string> _products = new List<string>();
            private long _cartNumber;
            private bool _isConfirmed;

            public CartActor(PID bank) => _bank = bank;

            public async Task ReceiveAsync(IContext ctx)
            {
                switch (ctx.Message)
                {
                    case AssignCartNumber assign:
                        _cartNumber = assign.CartNumber;
                        break;

                    case AddItem orderItem:
                        _products.Add(orderItem.ProductName);
                        ctx.Respond(new CartNumberResponse { CartNumber = _cartNumber });
                        break;

                    case ConfirmOrder confirmOrder:
                        _isConfirmed = true;
                        ctx.Respond(new ConfirmOrderResponse { OrderNumber = confirmOrder.CartNumber });
                        break;

                    case SendPaymentDetails _:
                        if (!_isConfirmed) throw new InvalidOperationException("Haaaaaa!");

                        var response = await ctx.RequestAsync<ProcessPaymentResponse>(_bank, new ProcessPayment());
                        if (!response.Ok) throw new InvalidOperationException("Haaaaaaaa!");

                        ctx.Respond(response);
                        break;
                }
            }
        }
    }
}