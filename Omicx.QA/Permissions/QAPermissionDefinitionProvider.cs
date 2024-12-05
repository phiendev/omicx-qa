using Omicx.QA.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Omicx.QA.Permissions;

public class QAPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(QAPermissions.GroupName);


        //Define your own permissions here. Example:
        //myGroup.AddPermission(QAPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<QAResource>(name);
    }
}
