using System;

namespace GeneratedCode
{
	public class Generate
	{
		/*
				CompilationUnit()
		.WithMembers(
			SingletonList<MemberDeclarationSyntax>(
				ClassDeclaration("LockStepAction")
				.WithMembers(
					SingletonList<MemberDeclarationSyntax>(
						MethodDeclaration(
							GenericName(
								Identifier("Action"))
							.WithTypeArgumentList(
								TypeArgumentList(
									SingletonSeparatedList<TypeSyntax>(
										IdentifierName("BinaryReader")))),
							Identifier("Create"))
						.WithModifiers(
							TokenList(

								new []{
									Token(SyntaxKind.PublicKeyword),
									Token(SyntaxKind.StaticKeyword)}))
						.WithTypeParameterList(
							TypeParameterList(
								SingletonSeparatedList<TypeParameterSyntax>(
									TypeParameter(
										Identifier("T")))))
						.WithParameterList(
							ParameterList(
								SingletonSeparatedList<ParameterSyntax>(
									Parameter(
										Identifier("action"))
									.WithType(
										GenericName(
											Identifier("Action"))
										.WithTypeArgumentList(
											TypeArgumentList(
												SeparatedList<TypeSyntax>(

													new SyntaxNodeOrToken[]{
														PredefinedType(
															Token(SyntaxKind.UIntKeyword)),
														Token(SyntaxKind.CommaToken),
														IdentifierName("T")})))))))
						.WithBody(
							Block(
								SingletonList<StatementSyntax>(
									ReturnStatement(
										AnonymousMethodExpression()
										.WithParameterList(
											ParameterList(
												SingletonSeparatedList<ParameterSyntax>(
													Parameter(
														Identifier("reader"))
													.WithType(
														IdentifierName("BinaryReader")))))
										.WithBody(
											Block(
												LocalDeclarationStatement(
													VariableDeclaration(
														PredefinedType(
															Token(SyntaxKind.UIntKeyword)))
													.WithVariables(
														SingletonSeparatedList<VariableDeclaratorSyntax>(
															VariableDeclarator(
																Identifier("SenderId"))
															.WithInitializer(
																EqualsValueClause(
																	InvocationExpression(
																		MemberAccessExpression(
																			SyntaxKind.SimpleMemberAccessExpression,
																			IdentifierName("reader"),
																			IdentifierName("ReadUInt32")))))))),
												LocalDeclarationStatement(
													VariableDeclaration(
														IdentifierName("T"))
													.WithVariables(
														SingletonSeparatedList<VariableDeclaratorSyntax>(
															VariableDeclarator(
																Identifier("Param1"))
															.WithInitializer(
																EqualsValueClause(
																	InvocationExpression(
																		MemberAccessExpression(
																			SyntaxKind.SimpleMemberAccessExpression,
																			IdentifierName("LockStepAction"),
																			GenericName(
																				Identifier("Read"))
																			.WithTypeArgumentList(
																				TypeArgumentList(
																					SingletonSeparatedList<TypeSyntax>(
																						IdentifierName("T"))))))))))),
												ExpressionStatement(
													InvocationExpression(
														IdentifierName("action"))
													.WithArgumentList(
														ArgumentList(
															SeparatedList<ArgumentSyntax>(

																new SyntaxNodeOrToken[]{
																	Argument(
																		IdentifierName("SenderId")),
																	Token(SyntaxKind.CommaToken),
																	Argument(
																		IdentifierName("Param1"))}))))))))))))))
		.NormalizeWhitespace()
		*/
	}

}
