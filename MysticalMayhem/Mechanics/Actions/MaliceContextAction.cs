using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Owlcat.QA.Validation;
using System.Linq;

namespace MysticalMayhem.Mechanics.Actions
{
    [TypeId("d1988c4c-6273-4ece-9568-77e8038bdef7")]
    [AllowedOn(typeof(BlueprintFeature), false)]
    public class MaliceContextAction : ContextActionDealDamage, IValidated, IDealDamageProvider
    {
        private readonly string[] _validHexes = new string[]
        {
            "28eb925896cfc83488d222b57a508bc3", // Death Curse
			"3dd413240a9cf614bbd48dacb172d048", // Evil Eye (AC)
			"58ce9c1ad7596c64a8a0e1fd4395c2c7", // Evil Eye (AB)
			"71e20ae6290617b428d03a8047874ea7", // Evil Eye (Saves)
			"101d5ae5477029e4bac167b3557cb25f", // Misfortune
			"d36e15046cc86c0418c69b8e13f29602", // Slumber
			"e6f3caca845caad469cde3a6187d4118", // Vulnerability Curse
			"7e53a38f2df6bc645bbcb8a6ffb0fd19", // Agony
			"e8c0fcf603dc0ea44a0b27624699bf17", // Delicious Fright
			"11a4a94204505d84e97154b2fdda234c", // Hoarfrost
			"1cf03f5321c4450498b1813f1a72ac82" // Restless Slumber
		};

        public override void RunAction()
        {
            if (Target?.Unit?.Descriptor
                .Facts
                .GetAll<Buff>()
                .Where(b => b.MaybeContext.MaybeCaster == Context.MaybeCaster && _validHexes.Contains(b.Blueprint.AssetGuidThreadSafe))
                .Count() > 0)
            {
                base.RunAction();
            }
        }
    }
}