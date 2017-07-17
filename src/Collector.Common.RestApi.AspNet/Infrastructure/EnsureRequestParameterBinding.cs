namespace Collector.Common.Infrastructure.WebApi.Infrastructure
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;
    using System.Web.Http.Metadata;

    public class EnsureRequestParameterBinding : HttpParameterBinding
    {
        public EnsureRequestParameterBinding(HttpParameterDescriptor descriptor)
            : base(descriptor)
        {
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var tsc = new TaskCompletionSource<object>();
            tsc.SetResult(null);
            var value = GetValue(actionContext) ?? new RequestBuilder(requestType: Descriptor.ParameterType).Create();
            SetValue(actionContext, value);
            return tsc.Task;
        }
    }
}