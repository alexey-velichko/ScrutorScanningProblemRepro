using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ScrutorScanningProblemRepro
{
    public interface IValidator<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task Validate(TRequest request, CancellationToken cancellationToken);
    }

    public class SayHelloRequest : IRequest<string>
    {
        public string Name { get; }
        public SayHelloRequest(string name)
        {
            Name = name;
        }
    }

    public class SayHelloValidator : IValidator<SayHelloRequest, string>
    {
        public async Task Validate(SayHelloRequest request, CancellationToken cancellationToken)
        {
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var typeToResolve =
                typeof(IValidator<,>).MakeGenericType(typeof(SayHelloRequest), typeof(string));

            var csp1 = new ServiceCollection()
                       .Scan(
                           c => c.FromExecutingAssembly()
                                 .AddClasses(classes => classes.AssignableTo(typeof(IValidator<,>)))
                                 .AsImplementedInterfaces()
                                 .WithSingletonLifetime()
                       )
                       .BuildServiceProvider();

            // this doesn`t resolve well:
            var validator1 = csp1.GetService(typeToResolve);

            Console.WriteLine($"Validator1 is null: {validator1 == null}");


            var csp2 = new ServiceCollection()
                       .AddSingleton<IValidator<SayHelloRequest, string>, SayHelloValidator>()
                       .BuildServiceProvider();

            // and this does:
            var validator2 = csp2.GetService(typeToResolve);

            Console.WriteLine($"Validator2 is null: {validator2 == null}");

            Console.ReadKey();
        }
    }
}
