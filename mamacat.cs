using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace thelost
{
    public class mamacat
    {
       
        static readonly PlayerFeature<bool> Mamafluff = PlayerBool("mamafluff");
        public void Hooks()
        {
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.Player.ctor += mamafluffdata;
        }

        private void mamafluffdata(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if(Mamafluff.TryGet(self,out bool value) && value)
            {
                Fluffdata fluffy = new Fluffdata();
                fluff.Add(self, fluffy);
            }
        }

        private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
        {
            orig(self, sLeaser, rCam,timeStacker,camPos);
            if(Mamafluff.TryGet(self.player, out bool mamafluff)&& mamafluff && sLeaser.sprites.Length > (ModManager.MSC ? 13 : 12))
            {
                fluff.TryGetValue(self.player, out var fluffinfo);
                Vector2 neckpos = Vector2.Lerp(sLeaser.sprites[0].GetPosition(), sLeaser.sprites[3].GetPosition(), 0.5f);
                sLeaser.sprites[fluffinfo.initialsprite].x = neckpos.x;
                sLeaser.sprites[fluffinfo.initialsprite].y = neckpos.y;
                //neck fluff done.

                for (int i = 1; i < sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[fluffinfo.initialsprite + i].x = self.tail[i - 1].pos.x;
                    sLeaser.sprites[fluffinfo.initialsprite + i].y = self.tail[i - 1].pos.y;
                }
            }
        }

        private void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (Mamafluff.TryGet(self.owner as Player, out bool mamafluff) && mamafluff)
            {

                fluff.TryGetValue(self.player, out var fluffinfo);
                fluffinfo.initialsprite = sLeaser.sprites.Length;
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 5);
                // 1 for neck fluff, 4 for tail fluffs.
                sLeaser.sprites[fluffinfo.initialsprite] = new FSprite("Circle20"); //neck fluff
                for (int i = 1; i < sLeaser.sprites.Length; i++) //tail fluff stuff
                {
                    sLeaser.sprites[fluffinfo.initialsprite + i] = new FSprite("Circle20");
                }
                self.AddToContainer(sLeaser, rCam, null);
                fluffinfo.ready = true;
            }
        }
        ConditionalWeakTable<Player, Fluffdata> fluff = new ConditionalWeakTable<Player, Fluffdata>();
        class Fluffdata
        {
            public int initialsprite;
            public string sprite;
            public bool ready = false;

        }
    }
}
