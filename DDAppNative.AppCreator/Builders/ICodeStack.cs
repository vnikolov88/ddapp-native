namespace DDAppNative.AppCreator.Builders
{
    interface ICodeStack
    {
        AppBuildState PrepareCodeBase(AppBuildState state);
        void FillInCode(AppBuildState state);
    }
}
