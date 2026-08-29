using Exiled.API.Enums;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;

namespace SCPCanvasPaint
{
    public class ArtistBrush : CustomItem
    {
        public override uint Id { get; set; } = 45100;
        public override string Name { get; set; } = "ArtistBrush";
        public override string Description { get; set; } = "[Кисть художника] Используйте для рисования на холстах. Выбросьте для включения режима.";
        public override float Weight { get; set; } = 1f;
        public override ItemType Type { get; set; } = ItemType.Medkit;

        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
        {
            Limit = 0
        };
    }
}
