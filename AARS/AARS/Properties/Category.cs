using KSP.UI.Screens;
using System.Collections.Generic;
using UnityEngine;

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Category : MonoBehaviour
    {
        private static readonly List<AvailablePart> availableParts = new List<AvailablePart>();

        void Awake()
        {
            GameEvents.onGUIEditorToolbarReady.Add(AARSCategoryFunc);
        }

        void AARSCategoryFunc()
        {
            const string customCategoryName = "AARS";
            const string customDisplayCategoryName = "AARS Parts";
            availableParts.Clear();
            availableParts.AddRange(PartLoader.LoadedPartsList.AARSParts());
            Texture2D iconTex = GameDatabase.Instance.GetTexture("AARS/Textures/AARSIcon", false);
            RUI.Icons.Selectable.Icon icon = new RUI.Icons.Selectable.Icon("AARS", iconTex, iconTex, false);
            PartCategorizer.Category filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == "Filter by Function");

            if (filter == null)
                Debug.Log("filter is null");
            else
                PartCategorizer.AddCustomSubcategoryFilter(filter, customCategoryName, customDisplayCategoryName, icon, p => availableParts.Contains(p));
        }
    }