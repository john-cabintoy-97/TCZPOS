using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Extension
{
    public enum AlertTypeList { Success, Error, Warning, Info }
    public class AlertType
    {
        public string Message { get; set; } = string.Empty;
        public AlertTypeList Type { get; set; } = AlertTypeList.Info;
        public bool IconOnly { get; set; } = false;
        public bool IsConfirm { get; set; } = false;
        public TaskCompletionSource<bool>? ConfirmTask { get; set; }

        public string ConfirmText { get; set; } = "Confirm";
        public string CancelText { get; set; } = "Dismiss";

        public string Icon => Type switch
        {
            AlertTypeList.Success => "fas fa-check-circle",
            AlertTypeList.Error => "fas fa-times-circle",
            AlertTypeList.Warning => "fas fa-exclamation-triangle",
            _ => "fas fa-info-circle"
        };
    }
}
