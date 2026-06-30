namespace ADHDTraining.Core.Input
{
    public static class AnalogKeyboardSourcePicker
    {
        public static IAnalogKeyboardSource CreateBest()
        {
            var wooting = new WootingAnalogKeyboardSource();
            if (wooting.IsAvailable) return wooting;
            var hid = new HidTierKeyboardSource();
            if (hid.IsAvailable) return hid;
            return new StandardKeyboardSource();
        }
    }
}
