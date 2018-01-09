﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.DynamicProxy.Test
{
    public class InterfaceInterceptingProxyClassGeneratorFixture
    {
        [Fact]
        public void ReturnGeneralType()
        {
            InterceptorDelegate interceptor = next => (async context =>
            {
                context.Arguments[0] = (int)context.Arguments[0] + 1;  
                await next(context);
            });
            var method = ReflectionUtility.GetMethod<ICalculator>(_ => _.Add(1, 2));
            var methodBasedInterceptor = new MethodBasedInterceptorDecoration(method, interceptor);
            var decoration = new InterceptorDecoration(new MethodBasedInterceptorDecoration[] { methodBasedInterceptor }, null);
            var generator = new InterfaceInterceptingProxyClassGenerator();
            var proxyType = generator.GenerateProxyClass(typeof(ICalculator), decoration);
            var proxy = (ICalculator)Activator.CreateInstance(proxyType, new Calculator(), decoration);
            Assert.Equal(4, proxy.Add(1, 2));
        }

        [Fact]
        public void RefOutParameter()
        {
            InterceptorDelegate interceptor = next => (async context =>
            {
                context.Arguments[0] = (int)context.Arguments[0] + 1;
                await next(context);
            });
            int x = 1;
            double y = 2;
            double result ;
            var method = ReflectionUtility.GetMethod<ICalculator>(_ => _.Substract(ref x, ref y, out result));
            var methodBasedInterceptor = new MethodBasedInterceptorDecoration(method, interceptor);
            var decoration = new InterceptorDecoration(new MethodBasedInterceptorDecoration[] { methodBasedInterceptor }, null);
            var generator = new InterfaceInterceptingProxyClassGenerator();
            var proxyType = generator.GenerateProxyClass(typeof(ICalculator), decoration);
            var proxy = (ICalculator)Activator.CreateInstance(proxyType, new Calculator(), decoration);
            proxy.Substract(ref x, ref y, out result);
            Assert.Equal(0, result);
            Assert.Equal(3, y);
        }

        public interface ICalculator
        {
            int Add(int  x, int y);
            void Substract(ref int x, ref double y,out double result);
        }

        public class Calculator : ICalculator
        {
            public int Add(int x, int y)
            {
                return x + y;
            }

            public  void Substract(ref int x, ref double y, out double result)
            {
                result =  x - y;
                y++;           
            }
        }
    }
}
