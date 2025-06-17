namespace ET.Client
{
    [Invoke(EYIUIInvokeType.Sync)]
    public class YIUIInvokeI2LocalizationEntitySyncHandler : AInvokeEntityHandler<EventView_ChangeLanguage>
    {
        public override void Handle(Entity entity, EventView_ChangeLanguage args)
        {
            entity?.DynamicEvent(args).NoContext();
        }
    }
}