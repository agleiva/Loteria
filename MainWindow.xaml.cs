using HtmlAgilityPack;
using System.Windows.Media;

namespace Loteria
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Load();
        }

        private async void Load()
        {
            DataContext = await Task.Run(() =>
            {
                var url = "https://www.loteriasyapuestas.es/es/resultados";
                var web = new HtmlWeb();
                var doc = web.Load(url);

                var query =
                    from n in doc.DocumentNode.Descendants("a")
                    where n.ContainsClass("__enlace-cabecera")
                    let titulo = n.Descendants("p")
                                  .FirstOrDefault(x => x.ContainsClass("c-ultimo-resultado__titulo"))
                                  ?.InnerText
                    where titulo != null
                    let fecha =
                        n.Descendants("p")
                         .FirstOrDefault(x => x.ContainsClass("c-ultimo-resultado__fecha"))
                         ?.InnerText
                    let numeros =
                        n.ParentNode
                         .Descendants("ul")
                         .Skip(1)
                         .FirstOrDefault()
                         ?.Descendants("li")
                         .Select(x => x.InnerText)
                         .Select(x => int.Parse(x))
                         .ToArray()
                    select (titulo, fecha, numeros);

                return
                   query.Take(4)
                        .Zip(new[] { Brushes.Green, Brushes.Red, Brushes.Blue, Brushes.Brown })
                        .Select(x => new Resultado(x.First.titulo, x.First.fecha, x.First.numeros, x.Second))
                        .ToList();
            });
            ProgressRing.Visibility = System.Windows.Visibility.Collapsed;
        }
    }

    public record Resultado(string Nombre, string Fecha, int[] Numeros, Brush Color);
}

public static class HtmlNodeExtensions
{
    public static bool ContainsClass(this HtmlNode node, string text) =>
        node.GetAttributeValue("class", "").Contains(text);
}
