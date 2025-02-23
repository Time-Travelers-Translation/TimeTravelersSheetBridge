using Logic.Domain.Kuriimu2.KomponentAdapter.Contract.DataClasses;

namespace Logic.Business.InjectionManagement.DataClasses.Scene
{
    public class ScnHeader
    {
        [FixedLength(8)]
        public string magic;
        public int unk1;
        public int unk2;
        public short unk3;
        public short unk4;
        public short unk5;
        public short unk6;
        public int sceneEntryCount;
        public int selectionCount;
        public int quickTimeCount;
        public int flagCount;
        public int counterCount;
        public int sceneOffset;
        public int decisionOffset;
        public int badEndOffset;
        public int hintOffset;
    }
}
