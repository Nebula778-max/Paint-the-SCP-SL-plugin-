using Exiled.API.Interfaces;
using System.ComponentModel;

namespace SCPCanvasPaint
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        [Description("Максимальная дистанция для рисования рейкастом")]
        public float MaxDrawDistance { get; set; } = 10f;

        [Description("Интервал обновления корутины рисования (в секундах)")]
        public float DrawInterval { get; set; } = 0.02f;

        [Description("Максимально допустимый размер матрицы (разрешение холста). 0 — без лимита.")]
        public int MaxMatrixSize { get; set; } = 100;

        [Description("Максимально допустимый физический размер холста (в метрах). 0 — без лимита.")]
        public float MaxPhysicalSize { get; set; } = 10f;
    }
}
