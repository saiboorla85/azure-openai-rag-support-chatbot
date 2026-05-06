using Microsoft.SemanticKernel;

namespace PropertySupport.RagApi.Services;

public interface IKernelBuilderService
{
    Kernel BuildKernel();
}
