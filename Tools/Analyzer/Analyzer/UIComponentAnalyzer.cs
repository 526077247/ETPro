using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UIComponentAnalyzer:DiagnosticAnalyzer
    {
        private const string Title = "UI类添加或获取UI组件类型错误";

        private const string MessageFormat = "组件类型: {0} 不允许作为实体: {1} 的UI子组件类型! 若要允许该类型作为参数,请使用UIComponentAttribute对孩子类标记";
        private const string MessageFormat2 = "类型: {0} 不允许调用UI组件的方法! 若要允许该类型请使用UIComponentAttribute对其标记";

        private const string Description = "实体类添加或获取UI组件类型错误.";
        private const string Description2 = "实体类添加或获取Gameobject类型错误.";

        private static readonly string[] UIComponentMethod = {"AddUIComponent","GetUIComponent","OpenWindow","CloseWindow"};
        private static readonly string[] GetTransfromMethod = {"GetTransform","GetGameObject"};
        
        private const string EntityType = "ET.Entity";
        
        private static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.UIComponentAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.Hotfix,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
        
        private static readonly DiagnosticDescriptor Rule2 =
                new DiagnosticDescriptor(DiagnosticIds.GetGameobjectAnalyzerRuleId,
                    Title,
                    MessageFormat2,
                    DiagnosticCategories.Hotfix,
                    DiagnosticSeverity.Error,
                    true,
                    Description2);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule,Rule2);

        
        public override void Initialize(AnalysisContext context)
        {
            if (!AnalyzerGlobalSetting.EnableAnalyzer)
            {
                return;
            }
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(this.AnalyzeMemberAccessExpression, SyntaxKind.SimpleMemberAccessExpression);
        }

        private void AnalyzeMemberAccessExpression(SyntaxNodeAnalysisContext context)
        {
            if (!AnalyzerHelper.IsAssemblyNeedAnalyze(context.Compilation.AssemblyName, AnalyzeAssembly.AllHotfix))
            {
                return;
            }

            if (!(context.Node is MemberAccessExpressionSyntax memberAccessExpressionSyntax))
            {
                return;
            }
            
            // 筛选出 Child函数syntax
            string methodName = memberAccessExpressionSyntax.Name.Identifier.Text;
            
            if (!UIComponentMethod.Contains(methodName)&&!GetTransfromMethod.Contains(methodName))
            {
                return;
            }
            
            if (!(memberAccessExpressionSyntax?.Parent is InvocationExpressionSyntax invocationExpressionSyntax) ||
                !(context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol is IMethodSymbol addChildMethodSymbol))
            {
                return;
            }
            // 获取函数的调用者类型
            ITypeSymbol? parentTypeSymbol = memberAccessExpressionSyntax.GetMemberAccessSyntaxParentType(context.SemanticModel);
            if (parentTypeSymbol==null)
            {
                return;
            }
            
            if (GetTransfromMethod.Contains(methodName))
            {
                // 只检查Entity的子类
                if (parentTypeSymbol.ToString() != EntityType)
                {
                    bool hasTypeAttribute = false;
                    foreach (AttributeData? attributeData in parentTypeSymbol.GetAttributes())
                    {
                        if (attributeData.AttributeClass?.Name == "UIComponentAttribute")
                        {
                            hasTypeAttribute = true;
                            break;
                        }
                    }

                    if (!hasTypeAttribute)
                    {
                        Diagnostic diagnostic = Diagnostic.Create(Rule2, memberAccessExpressionSyntax?.Name.Identifier.GetLocation(),
                            parentTypeSymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                        return;
                    }
                }
            }
            
            // 只检查Entity的子类
            if (parentTypeSymbol.BaseType?.ToString()!= EntityType)
            {
                return;
            }
            
            
            
            // 获取 Child实体类型
            ISymbol? ChildTypeSymbol = null;
            
            // Child为泛型调用
            if (addChildMethodSymbol.IsGenericMethod)
            {
                GenericNameSyntax? genericNameSyntax = memberAccessExpressionSyntax?.GetFirstChild<GenericNameSyntax>();

                TypeArgumentListSyntax? typeArgumentList = genericNameSyntax?.GetFirstChild<TypeArgumentListSyntax>();

                var ChildTypeSyntax = typeArgumentList?.Arguments.First();
                
                if (ChildTypeSyntax == null)
                {
                    Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                    throw new Exception("ChildTypeSyntax==null");
                }

                ChildTypeSymbol = context.SemanticModel.GetSymbolInfo(ChildTypeSyntax).Symbol;
            }
            //Child为非泛型调用
            

            if (ChildTypeSymbol==null)
            {
                return;
            }

            // 忽略 Child类型为泛型类型
            if (ChildTypeSymbol is ITypeParameterSymbol typeParameterSymbol)
            {
                return;
            }
            
            // 忽略 Type参数
            if (ChildTypeSymbol.ToString()=="System.Type")
            {
                return;
            }

            // 孩子类型为Entity时 忽略检查
            if (ChildTypeSymbol.ToString()== EntityType)
            {
                return;
            }
            
            // 判断Child类型是否属于约束类型

            //获取Child类的parentType标记数据
            INamedTypeSymbol? availableParentTypeSymbol = null;
            bool hasParentTypeAttribute = false;
            foreach (AttributeData? attributeData in ChildTypeSymbol.GetAttributes())
            {

                if (attributeData.AttributeClass?.Name == "UIComponentAttribute")
                {
                    hasParentTypeAttribute = true;
                    if (attributeData.ConstructorArguments.Length>0 && attributeData.ConstructorArguments[0].Value is INamedTypeSymbol typeSymbol)
                    {
                        availableParentTypeSymbol = typeSymbol;
                        break;
                    }
                }
            }

            if (hasParentTypeAttribute&&availableParentTypeSymbol==null)
            {
                return;
            }

            // 符合约束条件 通过检查
            if (availableParentTypeSymbol!=null && availableParentTypeSymbol.ToString()==parentTypeSymbol.ToString())
            {
                return;
            }

            {
                Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation(), ChildTypeSymbol?.Name,
                    parentTypeSymbol?.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}