using Microsoft.Xna.Framework.Audio;
using System.IO;
using System.Text;

namespace Engine.Util
{
    /// <summary>
    /// Methods to load audio files into SoundEffect objects.
    /// </summary>
    public class AudioHelper
    {
        public static SoundEffect SoundEffectFromWav(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            SoundEffect fx = SoundEffect.FromStream(fs);
            fs.Dispose();
            return fx;
        }

        public static SoundEffect SoundEffectFromOgg(string path)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                WavStreamFromOgg(path, ms);
                ms.Position = 0;
                SoundEffect fx = SoundEffect.FromStream(ms);
                //ms.Dispose();
                return fx;
            }
        }

        public static SoundEffect SoundEffectFromOgz(string path)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Muddle.Decrypt(path, out Stream oggStream);
                WavStreamFromOgg(oggStream, ms);
                oggStream.Dispose();
                ms.Position = 0;
                SoundEffect fx = SoundEffect.FromStream(ms);
                return fx;
            }
        }

        // This function is from a sample (NAudioDecodingTests.TestRaw()) in renaudbedard's fork of NVorbis.
        /// <summary>
        /// Convert ogg data from file to easily playable wav data.
        /// </summary>
        /// <param name="path">Path to ogg file.</param>
        /// <param name="wavStream">Stream to write wav data to.</param>
        static void WavStreamFromOgg(string path, Stream wavStream)
        {
            using (Stream oggStream = new FileStream(path, FileMode.Open))
            {
                WavStreamFromOgg(oggStream, wavStream);
            }
        }

        /*
        static void WavStreamFromOgz(string path, Stream wavStream)
        {
            Muddle.Decrypt(path, out Stream oggStream);
            WavStreamFromOgg(oggStream, wavStream);
            oggStream.Dispose();
        }
        */

        static void WavStreamFromOgg(Stream oggStream, Stream wavStream)
        {
            // NVorbis is already included with MonoGame. Otherwise it'd have to be added as a NuGet package.
            //using (var vorbis = new NVorbis.VorbisReader(path))
            using (var vorbis = new NVorbis.VorbisReader(oggStream, false))
            //using (MemoryStream wavStream = new MemoryStream())
            using (var writer = new BinaryWriter(wavStream, Encoding.UTF8, true))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(0);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(18);
                writer.Write((short)1); // PCM format
                writer.Write((short)vorbis.Channels);
                writer.Write(vorbis.SampleRate);
                writer.Write(vorbis.SampleRate * vorbis.Channels * 2);  // avg bytes per second
                writer.Write((short)(2 * vorbis.Channels)); // block align
                writer.Write((short)16); // bits per sample
                writer.Write((short)0); // extra size

                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Flush();
                var dataPos = wavStream.Position;
                writer.Write(0);

                var buf = new float[vorbis.SampleRate / 10 * vorbis.Channels];
                int count;
                while ((count = vorbis.ReadSamples(buf, 0, buf.Length)) > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var temp = (int)(32767f * buf[i]);
                        if (temp > 32767)
                        {
                            temp = 32767;
                        }
                        else if (temp < -32768)
                        {
                            temp = -32768;
                        }
                        writer.Write((short)temp);
                    }
                }
                writer.Flush();

                writer.Seek(4, SeekOrigin.Begin);
                writer.Write((int)(wavStream.Length - 8L));

                writer.Seek((int)dataPos, SeekOrigin.Begin);
                writer.Write((int)(wavStream.Length - dataPos - 4L));

                writer.Flush();

                return;
            }
        }
    }
}
