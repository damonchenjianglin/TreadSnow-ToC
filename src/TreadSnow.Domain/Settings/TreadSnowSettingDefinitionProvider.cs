using Volo.Abp.Settings;

namespace TreadSnow.Settings;

public class TreadSnowSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(TreadSnowSettings.MySetting1));
    }
}
