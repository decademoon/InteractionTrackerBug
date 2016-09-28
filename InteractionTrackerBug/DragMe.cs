using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace InteractionTrackerBug
{
    public sealed class DragMe : Control
    {
        private InteractionTracker tracker;
        private VisualInteractionSource interactionSource;

        public DragMe()
        {
            DefaultStyleKey = typeof(DragMe);

            Loaded += DragMe_Loaded;
            PointerPressed += DragMe_PointerPressed;
        }

        private void DragMe_Loaded(object sender, RoutedEventArgs e)
        {
            var visual = ElementCompositionPreview.GetElementVisual(this);
            var compositor = visual.Compositor;

            tracker = InteractionTracker.Create(compositor);
            interactionSource = VisualInteractionSource.Create(visual);

            var snap = InteractionTrackerInertiaRestingValue.Create(compositor);
            snap.Condition = compositor.CreateExpressionAnimation("True");
            snap.RestingValue = compositor.CreateExpressionAnimation("0");

            tracker.MinPosition = new Vector3(float.NegativeInfinity, 0, 0);
            tracker.MaxPosition = new Vector3(float.PositiveInfinity, 0, 0);
            interactionSource.PositionXSourceMode = InteractionSourceMode.EnabledWithInertia;
            tracker.ConfigurePositionXInertiaModifiers(new InteractionTrackerInertiaModifier[] { snap });

            tracker.InteractionSources.Add(interactionSource);

            var exp = compositor.CreateExpressionAnimation();
            exp.Expression = "-tracker.Position";
            exp.SetReferenceParameter("tracker", tracker);
            visual.StartAnimation("Offset", exp);
        }

        private void DragMe_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(this));
            }
        }
    }
}
