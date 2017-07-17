namespace Collector.Common.Infrastructure.WebApi.Infrastructure
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;
    using System.Web.Http.Metadata;

    public class RestfulParameterBinding : HttpParameterBinding
    {
        public RestfulParameterBinding(HttpParameterDescriptor descriptor)
            : base(descriptor)
        {
        }

        public override bool WillReadBody => true;

        public override async Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            await Descriptor.BindWithFormatter().ExecuteBindingAsync(metadataProvider, actionContext, cancellationToken);
            await Descriptor.BindWithModelBinding().ExecuteBindingAsync(metadataProvider, actionContext, cancellationToken);
            await new EnsureRequestParameterBinding(descriptor: Descriptor).ExecuteBindingAsync(metadataProvider, actionContext, cancellationToken);
        }
    }
}