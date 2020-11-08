using XTI_App;

namespace XTI_WebApp.Api
{
    public sealed class CachedModifierCategory : IModifierCategory
    {
        private readonly ModifierCategoryName name;

        public CachedModifierCategory(IModifierCategory modCategory)
        {
            ID = modCategory.ID;
            name = modCategory.Name();
        }

        public EntityID ID { get; }

        public ModifierCategoryName Name() => name;
    }
}
