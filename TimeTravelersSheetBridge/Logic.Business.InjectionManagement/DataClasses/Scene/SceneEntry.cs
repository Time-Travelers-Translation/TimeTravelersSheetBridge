using Logic.Domain.Kuriimu2.KomponentAdapter.Contract.DataClasses;

namespace Logic.Business.InjectionManagement.DataClasses.Scene
{
    public class SceneEntry
    {
        public short sceneId;
        public int metaId;
        
        public object metaData;

        public int branchCount;
        public int branchCount2;
        
        public BranchEntry[] branchEntries;
    }

    public class NoData
    {

    }

    public class DecisionData
    {
        public short unk;
        public byte timerIdent;
        public byte stringCount;
        public int unk1;
        public int decisionTextOffset;
    }

    public class MetaData2
    {
        public short unk1;
        public short unk2;
        public short unk3;
    }

    public class BadEndData
    {
        public short unk1;
        public short unk2;
        public int titleOffset;
        public int hintOffset;
    }

    public class BranchEntry
    {
        public short sceneId;
        public short unk0;

        public int count0;
        public int count1;
        public long[] unk3;

        public int count2;
        public int count3;
        public int[] unk6;

        public int count4;
        public int count5;
        public int[] unk9;
    }
}
