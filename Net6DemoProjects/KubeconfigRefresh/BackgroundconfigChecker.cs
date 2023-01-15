using k8s;
using Microsoft.Extensions.Primitives;

namespace KubeconfigRefresh
{
    public class BackgroundconfigChecker : BackgroundService
    {
        public BackgroundconfigChecker(IConfiguration configuration)
        {
         ChangeToken.OnChange(
            () => configuration.GetReloadToken(),
            () => global::System.Console.WriteLine($"Configuration Changed...{DateTime.Now.ToString()}")
            );

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public void Demo()
        {
            // Load from in-cluster configuration:
            var config = KubernetesClientConfiguration.InClusterConfig();

            //// Use the config object to create a client.
            //var client = new Kubernetes(config);

            //var genericPods = new GenericClient(client, "", "v1", "pods");
            //var pods = await genericPods.ListNamespacedAsync<V1Confg>("default").ConfigureAwait(false);
            //foreach (var pod in pods.Items)
            //{
            //    Console.WriteLine(pod.Metadata.Name);
            //}
        }
    }
}
