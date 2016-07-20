﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;

using TrollRAT.Payloads;
using TrollRATActions;

namespace TrollRATPayloads.Payloads
{
    public class PayloadEarthquake : LoopingPayload
    {
        [DllImport("TrollRATNative.dll")]
        public static extern void payloadEarthquake(int delay, int power);

        private PayloadSettingNumber power = new PayloadSettingNumber(20, "Movement Factor", 2, 60, 1);

        public PayloadEarthquake() : base(4)
        {
            actions.Add(new PayloadActionClearScreen(this));
            settings.Add(power);

            name = "Earthquake (Shake Screen)";
        }

        protected override void execute()
        {
            payloadEarthquake((int)Delay, (int)power.Value);
        }
    }

    public class PayloadMeltingScreen : LoopingPayload
    {
        [DllImport("TrollRATNative.dll")]
        public static extern void payloadMeltingScreen(int xSize, int ySize, int power, int distance);

        private PayloadSettingNumber xSize = new PayloadSettingNumber(30, "X Size", 1, 200, 1);
        private PayloadSettingNumber ySize = new PayloadSettingNumber(8, "Y Size", 1, 200, 1);
        private PayloadSettingNumber power = new PayloadSettingNumber(10, "Power", 1, 40, 1);
        private PayloadSettingNumber distance = new PayloadSettingNumber(1, "Distance between bars", 1, 500, 1);

        public PayloadMeltingScreen() : base(4)
        {
            actions.Add(new PayloadActionClearScreen(this));
            settings.Add(xSize);
            settings.Add(ySize);
            settings.Add(distance);
            settings.Add(power);

            name = "Melting Screen";
        }

        protected override void execute()
        {
            payloadMeltingScreen((int)xSize.Value, (int)ySize.Value, (int)power.Value, (int)distance.Value);
        }
    }

    public class PayloadTrain : LoopingPayload
    {
        [DllImport("TrollRATNative.dll")]
        public static extern void payloadTrain(int xPower, int yPower);

        private PayloadSettingNumber xPower = new PayloadSettingNumber(10, "X Movement", -100, 100, 1);
        private PayloadSettingNumber yPower = new PayloadSettingNumber(0, "Y Movement", -100, 100, 1);

        public PayloadTrain() : base(5)
        {
            actions.Add(new PayloadActionClearScreen(this));
            settings.Add(xPower);
            settings.Add(yPower);

            name = "Train/Elevator Effect";
        }

        protected override void execute()
        {
            payloadTrain((int)xPower.Value, (int)yPower.Value);
        }
    }

    public class PayloadDrawPixels : LoopingPayload
    {
        [DllImport("TrollRATNative.dll")]
        public static extern void payloadDrawPixels(uint color, int power);

        private PayloadSettingNumber power = new PayloadSettingNumber(500, "Changed Pixels per Iteration", 1, 10000, 1);
        protected PayloadSettingSelect color = new PayloadSettingSelect(0, "Color",
            new string[] { "Black", "White", "Red", "Green", "Blue", "Random (Black/White)", "Random (RGB)" });

        private static readonly uint[] colors = new uint[] { 0x000000, 0xFFFFFF, 0x0000FF, 0x00FF00, 0xFF0000 };

        private Random rng = new Random();

        public PayloadDrawPixels() : base(1)
        {
            actions.Add(new PayloadActionClearScreen(this));

            settings.Add(power);
            settings.Add(color);

            name = "Draw Pixels on Screen";
        }

        protected override void execute()
        {
            uint c;

            if (color.Value == colors.Length)
                c = rng.NextDouble() > 0.5 ? colors[0] : colors[1];
            else if (color.Value == colors.Length + 1)
                c = (uint)rng.Next();
            else
                c = colors[color.Value];

            payloadDrawPixels(c, (int)power.Value);
        }
    }

    public class PayloadTTS : ExecutablePayload
    {
        protected class PayloadSettingVoice : PayloadSettingSelectBase
        {
            public InstalledVoice SelectedVoice => synth.GetInstalledVoices()[value];

            public PayloadSettingVoice(string title) : base(0, title) { }

            public override string[] Options
            {
                get
                {
                    return (from voice in synth.GetInstalledVoices()
                            select voice.VoiceInfo.Name).ToArray();
                }
                set { throw new NotImplementedException(); }
            }
        }

        protected class PayloadActionStop : SimplePayloadAction
        {
            public PayloadActionStop(Payload payload) : base(payload) { }

            public override string Icon => null;
            public override string Title => "Stop Speaking";

            public override string execute()
            {
                synth.SpeakAsyncCancelAll();
                return "void(0);";
            }
        }

        private PayloadSettingString message = new PayloadSettingString(
            "soi soi soi soi soi soi soi soi soi soi soi", "Message to speak");

        private PayloadSettingNumber rate = new PayloadSettingNumber(1, "Speed Rate", -10, 10, 1);
        private PayloadSettingNumber volume = new PayloadSettingNumber(100, "Volume", 0, 100, 1);

        private PayloadSettingVoice voice = new PayloadSettingVoice("TTS Voice");

        protected static SpeechSynthesizer synth = new SpeechSynthesizer();

        public PayloadTTS()
        {
            settings.Add(message);
            settings.Add(voice);
            settings.Add(volume);
            settings.Add(rate);

            actions.Add(new PayloadActionStop(this));

            synth.SetOutputToDefaultAudioDevice();

            name = "Play TTS Voice";
        }

        protected override void execute()
        {
            synth.Rate = (int)rate.Value;
            synth.Volume = (int)volume.Value;

            synth.SelectVoice(voice.SelectedVoice.VoiceInfo.Name);

            try
            {
                synth.Speak(message.Value);
            }
            catch (Exception) { }
        }
    }
}
