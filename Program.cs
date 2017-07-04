﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using EventPulish;

namespace EventPulish
{
    class Program
    {

        static void Main(string[] args)
        {
            var container = ContainerConfig.Configure();
            using (var scope = container.BeginLifetimeScope())
            {
                var publisher = scope.Resolve<IPublisher>();
                var employeeEvent = new CreateEmployeeEvent()
                {
                    FirstName = "cuong",
                    LastName = "ba"
                };

                publisher.Publish(employeeEvent);
                publisher.Publish(new CreateEmployeeEvent()
                {
                    LastName = "cuong2",
                    FirstName = "cuong3"

                });
                publisher.Publish(new CreateCandidateEvent()
                {
                    FirstName = "cuong",
                    LastName = "ba"
                });
                Console.ReadLine();
            }         
        }
    }
    public static class ContainerConfig
    {
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Publisher>().As<IPublisher>();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>();

            var dataAccess = Assembly.GetExecutingAssembly();
            builder.RegisterAssemblyTypes(dataAccess)
                   .Where(t => t.Name.EndsWith("Subscriber"))
                   .AsImplementedInterfaces();
            return builder.Build();
        }
    }

    #region Interfaces

    public interface IEvent
    {

    }

    public interface IPublisher
    {
        void Publish<T>(T agr);
    }

    public interface ISubscriber<T>
    {
        void EventHandle(T agr);
    }

    public interface IEventAggregator
    {
        List<ISubscriber<T>> Subscribers<T>();
    }

    #endregion

    #region Implements
    public class CreateEmployeeEvent
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class CreateCandidateEvent
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class Publisher : IPublisher
    {
        private IEventAggregator _eventAggregator;

        public Publisher(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public void Publish<T>(T agr)
        {
            _eventAggregator.Subscribers<T>().ForEach(subscriber => subscriber.EventHandle(agr));
        }
    }
    public class CreateEmployeeEventSubscriber : ISubscriber<CreateEmployeeEvent>
    {
        public void EventHandle(CreateEmployeeEvent agr)
        {
            Console.WriteLine("----------CreateEmployeeEventSubscriber----------");
            Console.WriteLine(agr.LastName);
            Console.WriteLine(agr.FirstName);
        }
    }

    public class CreateCandidateSubscriber : ISubscriber<CreateCandidateEvent>
    {
        public void EventHandle(CreateCandidateEvent agr)
        {
            Console.WriteLine("----------CreateCandidateEventSubscriber----------");
            Console.WriteLine(agr.LastName);
            Console.WriteLine(agr.FirstName);
        }
    }
    public class EventAggregator : IEventAggregator
    {
        public List<ISubscriber<T>> Subscribers<T>()
        {
            var type = typeof(IMyInterface);
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => type.IsAssignableFrom(p));
            var container = ContainerConfig.Configure();
            return container.Resolve<IEnumerable<ISubscriber<T>>>().ToList();
        }
    }
    #endregion


}


