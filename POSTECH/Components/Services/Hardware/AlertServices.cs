using TCZPOS.Components.Extension;


namespace TCZPOS.Components.Services.Hardware
{
    public class AlertServices
    {
        public event Action<AlertType>? OnShow;
        private TaskCompletionSource<bool>? _confirmTaskSource;
        public void Notify(string message, AlertTypeList type = AlertTypeList.Info, bool iconOnly = false)
        {
            OnShow?.Invoke(new AlertType { Message = message, Type = type, IconOnly = iconOnly });
        }
        public async Task<bool> ConfirmAsync(
            string message,
            AlertTypeList type = AlertTypeList.Info,
            string confirmBtnText = "Confirm",
            string cancelBtnText = "Dismiss")
                {
                    var tcs = new TaskCompletionSource<bool>();

                    OnShow?.Invoke(new AlertType
                    {
                        Message = message,
                        Type = type,
                        IsConfirm = true,
                        ConfirmTask = tcs,
                        ConfirmText = confirmBtnText,
                        CancelText = cancelBtnText
                    });

            return await tcs.Task;
        }
        public void SetConfirmResult(bool result)
        {
            _confirmTaskSource?.TrySetResult(result);
        }
        public void Success(string msg = "", bool iconOnly = false) => Notify(msg, AlertTypeList.Success, iconOnly);
        public void Error(string msg = "", bool iconOnly = false) => Notify(msg, AlertTypeList.Error, iconOnly);
        public void Warning(string msg = "", bool iconOnly = false) => Notify(msg, AlertTypeList.Warning, iconOnly);
    }
}
