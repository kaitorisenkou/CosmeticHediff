using RimWorld;
using System.IO;
using UnityEngine;
using Verse;

namespace CosmeticHediff {
    public class HediffComp_Cosmetic : HediffComp {
        public HediffCompProperties_Cosmetic PropsCosmetic {
            get {
                return this.props as HediffCompProperties_Cosmetic;
            }
        }
        Graphic graphic = null;
        public Graphic GetGraphic() {
            if (graphic == null)
                ResolveGraphic();
            return graphic;
        }

        public void ResolveGraphic() {
            string path = PropsCosmetic.graphicPath;
            if (PropsCosmetic.useBodytype) {
                BodyTypeDef bodyType = Pawn.story.bodyType ?? BodyTypeDefOf.Male;
                path += "_" + bodyType.defName;
            }
            Shader shader = PropsCosmetic.useWornGraphicMask ? ShaderDatabase.CutoutComplex : ShaderDatabase.Cutout;
            Vector2 drawSize = PropsCosmetic.drawSize;
            Color color = PropsCosmetic.color;

            this.graphic = GraphicDatabase.Get<Graphic_Multi>(path, shader, drawSize, color);
        }

    }
    public class HediffCompProperties_Cosmetic : HediffCompProperties {
        public HediffCompProperties_Cosmetic() {
            this.compClass = typeof(HediffComp_Cosmetic);
        }

        public CosmeticHediffLayer layer = CosmeticHediffLayer.apparel;
        public bool hideHair = false;
        public bool useBodytype = true;
        public bool useWornGraphicMask = false;
        public Vector2 drawSize = Vector2.one;
        public Color color = Color.white;
        [NoTranslate]
        public string graphicPath = "";
    }

    public enum CosmeticHediffLayer: byte {
        skin,
        apparel,
        head,
        overHeadgear
    }
}
