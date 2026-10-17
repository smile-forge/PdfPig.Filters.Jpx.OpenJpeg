using UglyToad.PdfPig.Tokens;

namespace UglyToad.PdfPig.Filters.Jpx.OpenJpeg.Tests
{
    /// <summary>
    /// An image whose PDF colour space is /Indexed reads its samples as indices into the PDF's own lookup table, which
    /// overrides any colour information in the JPEG 2000 data (ISO 32000-1, 7.4.9) — a palette in the codestream included.
    /// </summary>
    public class IndexedImageTest
    {
        private static readonly ParsingOptions _parsingOption = new ParsingOptions()
        {
            UseLenientParsing = true,
            SkipMissingFonts = true,
            FilterProvider = JpxFilterProvider.Instance
        };

        [Fact]
        public void AnIndexedImageKeepsItsPaletteIndices()
        {
            // The image is a codestream of 4-bit samples with a 16-entry palette of its own, under [/Indexed /ICCBased 15 …].
            using (var doc = PdfDocument.Open(Path.Combine("Documents", "PDFBOX-4328-1.pdf"), _parsingOption))
            {
                var image = doc.GetPage(1).GetImages().Single(i =>
                    i.ImageDictionary.TryGet(NameToken.Filter, out NameToken filter) && filter.Equals(NameToken.JpxDecode));

                Assert.True(image.TryGetBytesAsMemory(out var samples));

                // One index a sample, each row whole bytes — not the codestream's palette applied, three colours a sample.
                var row = (image.WidthInSamples * image.BitsPerComponent + 7) / 8;
                Assert.Equal(row * image.HeightInSamples, samples.Length);

                Assert.True(image.TryGetPng(out _));
            }
        }
    }
}
