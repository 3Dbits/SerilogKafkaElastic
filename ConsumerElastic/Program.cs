// See https://aka.ms/new-console-template for more information
using Confluent.Kafka;
using Serilog.Sinks.Elasticsearch;
using Serilog;

Console.WriteLine("Hello, World!");

var config = new ConsumerConfig
{
    BootstrapServers = "localhost:9092", // Replace with your Kafka broker address
    GroupId = "my-consumer-group",
    AutoOffsetReset = AutoOffsetReset.Earliest,
    EnableAutoCommit = false
};

Log.Logger = new LoggerConfiguration()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
            {
                AutoRegisterTemplate = true,
            })
            .CreateLogger();

using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
{
    consumer.Subscribe("topic-test");

    Console.WriteLine("Consumer started. Press Ctrl+C to exit.");

    try
    {
        while (true)
        {
            var consumeResult = consumer.Consume();

            if (consumeResult.IsPartitionEOF)
            {
                Console.WriteLine($"Reached end of partition {consumeResult.Partition}, offset {consumeResult.Offset}.");
                continue;
            }

            Log.Information($"{consumeResult.Message.Value}");
            Console.WriteLine($"Received message: {consumeResult.Message.Value}");
        }
    }
    catch (OperationCanceledException)
    {
        // Ctrl+C was pressed
    }
    finally
    {
        consumer.Close();
    }
}