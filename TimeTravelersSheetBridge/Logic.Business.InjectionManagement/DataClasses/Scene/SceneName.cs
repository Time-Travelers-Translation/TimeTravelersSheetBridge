using System.Text;

namespace Logic.Business.InjectionManagement.DataClasses.Scene
{
    class SceneName
    {
        public byte[] sceneName;
        public short sceneId;

        public override string ToString()
        {
            return Encoding.ASCII.GetString(sceneName.TakeWhile(x => x != 0).ToArray());
        }
    }
}
