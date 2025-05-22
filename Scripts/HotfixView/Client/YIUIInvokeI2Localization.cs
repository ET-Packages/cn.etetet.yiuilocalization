namespace ET.Client
{
    [Invoke(EYIUIInvokeType.Sync)]
    public class YIUIInvokeI2LocalizationSyncHandler : AInvokeHandler<EventView_ChangeLanguage>
    {
        public override void Handle(EventView_ChangeLanguage args)
        {
            YIUILoadComponent.Inst?.DynamicEvent(args).NoContext();
        }
    }
}