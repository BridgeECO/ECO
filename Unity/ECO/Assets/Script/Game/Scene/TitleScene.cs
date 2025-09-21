namespace ECO
{
    public class TitleScene : SceneBase
    {
        //SerializeFied -> 디자이너 분들, 아트 분들이 편집해야 할 떄 (없업도 안터지는 것들)

        protected override bool OnSceneAwake()
        {
            //Componenet 설정 (코드에서 가져오는거)
            //버튼 이벤트 할당
            return true;
        }

        protected override void OnSceneDestroy()
        {
            //파괴
        }

        protected override void OnSceneFixeUpdate()
        {
            //업데이트
        }

        protected override void OnSceneUpdate()
        {
            //업데이트
        }
    }
}