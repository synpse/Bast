using Bast.Common.Enums;
using System.Collections.Generic;
using System.Security;

namespace Bast.Operations
{
    public interface IOperationManager
    {
        void LaunchOperation(OperationKind operationKind, List<string> inputFile, SecureString password);
    }
}