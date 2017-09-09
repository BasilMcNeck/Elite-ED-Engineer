using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.UI.Notifications;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Utils.System;
using EDEngineer.Views.Popups;
using Application = System.Windows.Application;

namespace EDEngineer.Views
{
    public class CommanderToasts : IDisposable
    {
        private readonly State state;
        private readonly string commanderName;
        private readonly BlockingCollection<ToastNotification> toasts = new BlockingCollection<ToastNotification>(); 
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        public CommanderToasts(State state, string commanderName)
        {
            this.state = state;
            this.commanderName = commanderName;
            Task.Factory.StartNew(ConsumeToasts);
        }

        private void ConsumeToasts()
        {
            var toDisplay = new HashSet<ToastNotification>();
            var fiveSeconds = TimeSpan.FromSeconds(5);
            while (!tokenSource.Token.IsCancellationRequested)
            {
                ToastNotification item;
                while (toasts.TryTake(out item, fiveSeconds) && toDisplay.Add(item));

                if (toDisplay.Count <= 2)
                {
                    foreach (var toast in toDisplay)
                    {
                        ToastNotificationManager.CreateToastNotifier("EDEngineer").Show(toast);
                    }
                }
                else
                {
                    var translator = Languages.Instance;

                    var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText01);

                    var stringElements = toastXml.GetElementsByTagName("text");

                    stringElements[0].AppendChild(toastXml.CreateTextNode(translator.Translate("Multiple Blueprints Ready")));

                    var imagePath = "file:///" + Path.GetFullPath("Resources/Images/elite-dangerous-clean.png");

                    var imageElements = toastXml.GetElementsByTagName("image");
                    imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

                    var toast = new ToastNotification(toastXml);
                    ToastNotificationManager.CreateToastNotifier("EDEngineer").Show(toast);
                }

                toDisplay.Clear();
            }
        }

        private void ThresholdToastCheck(string item)
        {
            if (!SettingsManager.ThresholdWarningEnabled)
            {
                return;
            }

            var translator = Languages.Instance;

            if (!state.Cargo.ContainsKey(item))
            {
                return;
            }

            var entry = state.Cargo[item];

            if (entry.Threshold.HasValue && entry.Threshold <= entry.Count)
            {
                try
                {
                    var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

                    var stringElements = toastXml.GetElementsByTagName("text");
                    var content = string.Format(
                        translator.Translate(
                            "Reached {0} {1} - threshold set at {2} (click this to configure your thresholds)"),
                        entry.Count, translator.Translate(entry.Data.Name), entry.Threshold);
                    stringElements[0].AppendChild(toastXml.CreateTextNode(translator.Translate("Threshold Reached!")));
                    stringElements[1].AppendChild(toastXml.CreateTextNode(content));

                    var imagePath = "file:///" + Path.GetFullPath("Resources/Images/elite-dangerous-clean.png");

                    var imageElements = toastXml.GetElementsByTagName("image");
                    imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

                    var toast = new ToastNotification(toastXml);
                    toast.Activated +=
                        (o, e) => Application.Current.Dispatcher.Invoke(() => ThresholdsManagerWindow.ShowThresholds(translator, state.Cargo, commanderName));

                    ToastNotificationManager.CreateToastNotifier("EDEngineer").Show(toast);
                }
                catch (Exception)
                {
                    // silently fail for platforms not supporting toasts
                }
            }
        }

        private void LimitToastCheck(string property)
        {
            if (!SettingsManager.CargoAlmostFullWarningEnabled)
            {
                return;
            }

            var translator = Languages.Instance;

            var ratio = state.MaxMaterials - state.MaterialsCount;
            string headerText, contentText;
            if (ratio <= 5 && property == "MaterialsCount")
            {
                headerText = translator.Translate("Materials Almost Full!");
                contentText = string.Format(translator.Translate("You have only {0} slots left for your materials."), ratio);
            }
            else if ((ratio = state.MaxData - state.DataCount) <= 5 && property == "DataCount")
            {
                headerText = translator.Translate("Data Almost Full!");
                contentText = string.Format(translator.Translate("You have only {0} slots left for your data."), ratio);
            }
            else
            {
                return;
            }

            try
            {
                var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

                var stringElements = toastXml.GetElementsByTagName("text");

                stringElements[0].AppendChild(toastXml.CreateTextNode(headerText));
                stringElements[1].AppendChild(toastXml.CreateTextNode(contentText));

                var imagePath = "file:///" + Path.GetFullPath("Resources/Images/elite-dangerous-clean.png");

                var imageElements = toastXml.GetElementsByTagName("image");
                imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

                var toast = new ToastNotification(toastXml);

                ToastNotificationManager.CreateToastNotifier("EDEngineer").Show(toast);
            }
            catch (Exception)
            {
                // silently fail for platforms not supporting toasts
            }
        }

        private void BlueprintOnFavoriteAvailable(object sender, EventArgs e)
        {
            if (!SettingsManager.BlueprintReadyToastEnabled)
            {
                return;
            }

            var blueprint = (Blueprint)sender;
            try
            {
                var translator = Languages.Instance;

                var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

                var stringElements = toastXml.GetElementsByTagName("text");

                stringElements[0].AppendChild(toastXml.CreateTextNode(translator.Translate("Blueprint Ready")));
                stringElements[1].AppendChild(toastXml.CreateTextNode($"{translator.Translate(blueprint.BlueprintName)} (G{blueprint.Grade})"));
                stringElements[2].AppendChild(toastXml.CreateTextNode($"{string.Join(", ", blueprint.Engineers)}"));

                var imagePath = "file:///" + Path.GetFullPath("Resources/Images/elite-dangerous-clean.png");

                var imageElements = toastXml.GetElementsByTagName("image");
                imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

                var toast = new ToastNotification(toastXml);

                toasts.Add(toast);
            }
            catch (Exception)
            {
                // silently fail for platforms not supporting toasts
            }
        }

        public void UnsubscribeToasts()
        {
            foreach (var blueprint in state.Blueprints)
            {
                blueprint.FavoriteAvailable -= BlueprintOnFavoriteAvailable;
            }

            state.PropertyChanged -= StateCargoCountChanged;
        }

        public void SubscribeToasts()
        {
            foreach (var blueprint in state.Blueprints)
            {
                blueprint.FavoriteAvailable += BlueprintOnFavoriteAvailable;
            }

            state.PropertyChanged += StateCargoCountChanged;
        }

        private void StateCargoCountChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MaterialsCount" || e.PropertyName == "DataCount")
            {
                LimitToastCheck(e.PropertyName);
            }

            ThresholdToastCheck(e.PropertyName);
        }

        public void Dispose()
        {
            tokenSource.Cancel();
        }
    }
}