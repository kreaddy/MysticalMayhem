using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using System.Collections.Generic;

namespace MysticalMayhem.Mechanics
{
    /// <summary>
    /// UnitPart monitoring the characters on-off features as added or removed.
    /// </summary>
    internal class FeatureExtender : OldStyleUnitPart
    {
        private readonly Dictionary<Feature, CountableFlag> List = new ();

        public enum Feature
        {
            AcadamaeGraduate,
            GreaterMortifiedCasting,
            MythicPurityOfSin,
            PactWizardAutoPassChecks,
            PactWizardSpellConversion,
            StaffLikeWand,
            EarthMagic,
            RazmiranChannel
        }

        public void AddFeature(Feature type)
        {
            CountableFlag feature = GetFeature(type);
            feature.Retain();
        }

        public void RemoveFeature(Feature type)
        {
            CountableFlag feature = GetFeature(type);
            feature.Release();
        }

        public void ClearFeature(Feature type)
        {
            CountableFlag feature = GetFeature(type);
            feature.ReleaseAll();
        }

        public CountableFlag GetFeature(Feature type)
        {
            CountableFlag feature;
            List.TryGetValue(type, out feature);
            if (feature == null)
            {
                feature = new CountableFlag();
                List[type] = feature;
            }
            return feature;
        }

        /// <summary>
        /// Child class of FeatureExtender meant to be put on blueprints in order to add an extra feature.
        /// </summary>
        [AllowMultipleComponents]
        [TypeId("a216ddc2-2465-4259-b317-663bec0c38d7")]
        public class AddExtraFeature : UnitFactComponentDelegate
        {
            public Feature Feature;

            public override void OnTurnOn()
            {
                Owner.Ensure<FeatureExtender>().AddFeature(Feature);
            }

            public override void OnTurnOff()
            {
                Owner.Ensure<FeatureExtender>().RemoveFeature(Feature);
            }
        }
    }

    // Small extension to UnitDescriptor to shorten calls.
    internal static class FeatureExtenderUnitEntityDataExtension
    {
        /// <summary>
        /// Fetch an extra feature from a UnitDescriptor.
        /// </summary>
        internal static CountableFlag GetEXFeature(this UnitDescriptor _instance, FeatureExtender.Feature type)
        {
            return _instance.Ensure<FeatureExtender>().GetFeature(type);
        }
    }
}