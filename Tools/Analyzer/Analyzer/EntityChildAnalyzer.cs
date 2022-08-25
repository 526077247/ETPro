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
    public class EntityChildAnalyzer:DiagnosticAnalyzer
    {
        private const string Title = "实体类添加或获取孩子类型错误";

        private const string MessageFormat = "孩子类型: {0} 不允许作为实体: {1} 的孩子类型! 若要允许该类型作为参数,请使用ChildOfAttribute对孩子类标记父级实体类型";

        private const string Description = "实体类添加或获取孩子类型错误.";

        private static readonly string[] ChildMethod = {"AddChild","GetChild"};
        
        private const string EntityType = "ET.Entity";
        
        private static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.EntityChildAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.Hotfix,
                    DiagnosticSeverity.Error,
                    true,
                    Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        
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

            if (!ChildMethod.Contains(methodName))
            {
                return;
            }
            
            if (!(memberAccessExpressionSyntax?.Parent is InvocationExpressionSyntax invocationExpressionSyntax) ||
                !(context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol is IMethodSymbol addChildMethodSymbol))
            {
                return;
            }
            
            // 获取AChild函数的调用者类型
            ITypeSymbol? parentTypeSymbol = memberAccessExpressionSyntax.GetMemberAccessSyntaxParentType(context.SemanticModel);
            if (parentTypeSymbol==null)
            {
                return;
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
            else
            {
                SyntaxNode? firstArgumentSyntax = invocationExpressionSyntax.GetFirstChild<ArgumentListSyntax>()?.GetFirstChild<ArgumentSyntax>()
                        ?.ChildNodes().First();
                if (firstArgumentSyntax == null)
                {
                    Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                    return;
                }

                // 参数为typeOf时 提取Type类型
                if (firstArgumentSyntax is TypeOfExpressionSyntax typeOfExpressionSyntax)
                {
                    firstArgumentSyntax = typeOfExpressionSyntax.Type;
                }

                ISymbol? firstArgumentSymbol = context.SemanticModel.GetSymbolInfo(firstArgumentSyntax).Symbol;

                if (firstArgumentSymbol is ILocalSymbol childLocalSymbol)
                {
                    ChildTypeSymbol = childLocalSymbol.Type;
                }
                else if (firstArgumentSymbol is IParameterSymbol childParamaterSymbol)
                {
                    ChildTypeSymbol = childParamaterSymbol.Type;
                }
                else if (firstArgumentSymbol is IMethodSymbol methodSymbol)
                {
                    ChildTypeSymbol = methodSymbol.ReturnType;
                }
                else if (firstArgumentSymbol is IFieldSymbol fieldSymbol)
                {
                    ChildTypeSymbol = fieldSymbol.Type;
                }
                else if (firstArgumentSymbol is IPropertySymbol propertySymbol)
                {
                    ChildTypeSymbol = propertySymbol.Type;
                }else if (firstArgumentSymbol is INamedTypeSymbol namedTypeSymbol)
                {
                    ChildTypeSymbol = namedTypeSymbol;
                }else if (firstArgumentSymbol is ITypeParameterSymbol)
                {
                    // 忽略typeof(T)参数类型
                    return;
                }
                else if (firstArgumentSymbol != null)
                {
                    Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation(),
                        firstArgumentSymbol.Name, parentTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
                else
                {
                    Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation(),
                        firstArgumentSyntax.GetText(), parentTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
            }

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

                if (attributeData.AttributeClass?.Name == "ChildOfAttribute")
                {
                    hasParentTypeAttribute = true;
                    if (attributeData.ConstructorArguments[0].Value is INamedTypeSymbol typeSymbol)
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