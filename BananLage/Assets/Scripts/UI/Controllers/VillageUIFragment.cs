using Mechanics.Village;
using UnityEngine.UIElements;

namespace UI.Controllers
{
    public class VillageUIFragment : UIFragment
    {
        public AdvanceCycleModalController modalController;
        protected override string RootId => "villageInfoRoot";

        protected override void OnCreateView()
        {
            var vo = new VillageViewObject();
            RootElement.dataSource = vo;

            var e = RootElement.Q("advanceCycle");
            e.RegisterCallback<ClickEvent>(AdvanceCycle);
            e.RegisterCallback<ClickEvent>(_ => SoundEngine.PlaySFX(SoundEngine.bus.Sounds.uiClick, transform));
        }

        private void AdvanceCycle(ClickEvent evt)
        {
            if (!modalController) return;

            VillageManager.AdvanceCycle(modalController);
        }
    }
}