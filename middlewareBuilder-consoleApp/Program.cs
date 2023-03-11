using System;
using System.IO;

namespace middlewareBuilder_consoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var directoryPath = @"C:\MyFolder";

            var middleware = new MiddlewareBuilder()
                .Use(new FileFinderMiddleware(directoryPath))
                .Use(new FileProcessorMiddleware())
                .Use(new ReportGeneratorMiddleware())
                .Build();

            middleware.Invoke(null);
        }
    }

    public class FileFinderMiddleware
    {
        private readonly string _directoryPath;
        private MiddlewareDelegate _next;

        public FileFinderMiddleware(string directoryPath)
        {
            _directoryPath = directoryPath;
        }

        public void Invoke(object context)
        {
            var files = Directory.GetFiles(_directoryPath, "*.txt");

            foreach (var file in files)
            {
                _next.Invoke(file);
            }
        }

        public void Use(MiddlewareDelegate next)
        {
            _next = next;
        }
    }

    public class FileProcessorMiddleware
    {
        private MiddlewareDelegate _next;

        public void Invoke(object context)
        {
            var filePath = (string)context;

            // Do some processing with the file
            Console.WriteLine($"Processing file: {filePath}");

            _next.Invoke(null);
        }

        public void Use(MiddlewareDelegate next)
        {
            _next = next;
        }
    }

    public class ReportGeneratorMiddleware
    {
        public void Invoke(object context)
        {
            // Generate a report based on the processed files
            Console.WriteLine("Generating report...");

            // No next middleware to call
        }

        public void Use(MiddlewareDelegate next)
        {
            // This is the last middleware in the chain
        }
    }

    public delegate void MiddlewareDelegate(object context);

    public class MiddlewareBuilder
    {
        private MiddlewareDelegate _first;

        public MiddlewareBuilder Use(object middleware)
        {
            var newMiddleware = (MiddlewareDelegate)middleware.GetType().GetMethod("Invoke").CreateDelegate(typeof(MiddlewareDelegate), middleware);

            if (_first == null)
            {
                _first = newMiddleware;
            }
            else
            {
                MiddlewareDelegate lastMiddleware = _first;
                while (lastMiddleware != null)
                {
                    if (lastMiddleware.Method.Name == "Use")
                    {
                        break;
                    }
                    lastMiddleware = (MiddlewareDelegate)lastMiddleware.Target.GetType().GetMethod("Invoke").CreateDelegate(typeof(MiddlewareDelegate), lastMiddleware.Target);
                }

                lastMiddleware.Use(newMiddleware);
            }

            return this;
        }

        public MiddlewareDelegate Build()
        {
            return _first;
        }
    }
}

