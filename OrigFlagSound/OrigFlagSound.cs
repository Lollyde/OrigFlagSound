using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using MonoMod.RuntimeDetour;
using On.Celeste;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.OrigFlagSound {
	public class OrigFlagSound : EverestModule {

		public static OrigFlagSound Instance;

		public override Type SettingsType => typeof(OrigFlagSoundSettings);
		static OrigFlagSoundSettings Settings => (OrigFlagSoundSettings)Instance._Settings;

		public OrigFlagSound() {
			Instance = this;
		}

		public override void Load() {
			On.Celeste.SummitCheckpoint.Update += hook_update;
		}

		public override void Unload() {
			On.Celeste.SummitCheckpoint.Update -= hook_update;
		}


		private void hook_update(On.Celeste.SummitCheckpoint.orig_Update orig, SummitCheckpoint self) {
			if (Settings.Enabled) {
				if (!self.Activated) {
					Player player = self.CollideFirst<Player>();
					if (player != null && player.OnGround(1) && player.Speed.Y >= 0f) {
						Level level = self.Scene as Level;
						self.Activated = true;
						level.Session.SetFlag("summit_checkpoint_" + self.Number, true);
						level.Session.RespawnPoint = new Vector2?((Vector2)typeof(SummitCheckpoint).GetField("respawn", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self));
						level.Session.UpdateLevelStartDashes();
						level.Session.HitCheckpoint = true;
						level.Displacement.AddBurst(self.Position, 0.5f, 4f, 24f, 0.5f, null, null);
						level.Add(new SummitCheckpoint.ConfettiRenderer(self.Position));
						Audio.Play("event:/lollyde/origflagsound", self.Position);
					}
				}
			} else {
				orig(self);
			}
		}
	}
}
