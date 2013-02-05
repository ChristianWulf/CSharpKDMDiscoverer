/***************************************************************************
 * Copyright 2012 by
 * + Christian-Albrechts-University of Kiel
 * + Department of Computer Science
 * + Software Engineering Group
 * and others.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ***************************************************************************/

package mapping.code.transformator;

import java.util.LinkedList;
import java.util.List;

import lang.csharp.CSharp4AST;
import mapping.KDMElementFactory;
import mapping.code.extern.TypeNotFoundException;
import mapping.code.language.GenericLanguageUnitCache;
import mapping.code.language.NotInCacheException;
import mapping.code.namespace.NamespaceStack;
import mapping.code.resolver.IdentifierResolver;

import org.antlr.runtime.tree.CommonTree;
import org.antlr.runtime.tree.Tree;
import org.eclipse.gmt.modisco.omg.kdm.code.ArrayType;
import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeFactory;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.CompilationUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CompositeType;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.InterfaceUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.ItemUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.MethodUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.PointerType;

import util.ListUtil;
import util.Logger;
import util.Pause;
import de.cau.chw.transformation.external.ExternalEntity;
import de.cau.chw.transformation.external.ResolutionMarker;

public class TypeTransformator {

	private static final Logger				LOGGER	= new Logger(TypeTransformator.class);

	private final IdentifierResolver		identifierResolver;
	private final GenericLanguageUnitCache	genericLanguageUnitCache;
	private final AbstractTransformator		transformator;

	public TypeTransformator(final IdentifierResolver identifierResolver,
			final GenericLanguageUnitCache genericLanguageUnitCache, final AbstractTransformator transformator) {
		this.identifierResolver = identifierResolver;
		this.genericLanguageUnitCache = genericLanguageUnitCache;
		this.transformator = transformator;
	}

	public Datatype transform(final CommonTree commonTreeNode, final CodeItem parent,
			final List<CodeItem> declarations, final NamespaceStack internalNamespaceStack,
			final CompilationUnit compilationUnit) throws TypeNotFoundException {

		Tree baseTypeNode = commonTreeNode.getChild(0);
		@SuppressWarnings("unchecked") // composite type (e.g. class or interface)
		List<String> qualifiedBaseType = transformator.walk(baseTypeNode, parent, List.class);

		// collect type extensions such as INTERR, rank_specifier, STAR
		List<Integer> entityEntensions = new LinkedList<Integer>();
		for (int i = 1; i < commonTreeNode.getChildCount(); i++) {
			Tree typeExtension = commonTreeNode.getChild(i);
			// INTERR, rank_specifier, STAR
			entityEntensions.add(typeExtension.getType());
			if (typeExtension.getType() == CSharp4AST.RANK_SPECIFIER) {
				entityEntensions.add(typeExtension.getChildCount());
			}
		}

		try {
			Datatype baseType = null;
			switch (baseTypeNode.getType()) {
				case CSharp4AST.NAMESPACE_OR_TYPE_NAME:
					if (qualifiedBaseType.size() > 1 || !qualifiedBaseType.get(0).equals("String")) {
						baseType = (Datatype) identifierResolver.resolveType(qualifiedBaseType, declarations,
								internalNamespaceStack, compilationUnit, parent);

						if (baseType == null) {
							LOGGER.info("Generic type found in " + parent.getName() + ": " + qualifiedBaseType);
							// baseType is a generic type. add it to the parent
							baseType = KDMElementFactory.createDatatype(ListUtil.combine(qualifiedBaseType, "."));
							addGenericToParent(parent, baseType);
						}
					} else {
						// handle type "String"
						try {
							baseType = genericLanguageUnitCache.getDatatypeFromString("String");
						} catch (NotInCacheException e) {
							throw new IllegalStateException(e.getMessage(), e);
						}
					}
					break;
				default: // OBJECT, STRING, VOID, IDENTIFIER(dynamic), and primitive
					// types
					try {
						baseType = genericLanguageUnitCache.getDatatypeFromString(baseTypeNode.getText());
					} catch (NotInCacheException e) {
						throw new IllegalStateException(e.getLocalizedMessage(), e);
					}
					break;
			}

			if (baseType == null) {
				throw new IllegalStateException("baseTypeNode: " + baseTypeNode + ", but baseType == null");
			}

			if (baseType.eContainer() == null) {
				compilationUnit.getCodeElement().add(baseType);
				LOGGER.warning("Added baseType '" + baseType + "' to CompilationUnit");
			}

			return wrapTypeExtensions(commonTreeNode, baseType);
		} catch (TypeNotFoundException e) {
			if (transformator instanceof Phase2TransformatorNew) {
				((Phase2TransformatorNew) transformator).fireUnknownEntityDetected(new ExternalEntity(
						qualifiedBaseType, entityEntensions), new ResolutionMarker(Datatype.class, parent));
			}
			// skip e.g. due to LOGGER.unsupported generic type
			return KDMElementFactory.createDatatype("LOGGER.unsupported: " + commonTreeNode.getChildren());
			// throw e; // TODO only for test purpose
		} catch (RuntimeException e) {
			if (transformator instanceof Phase2TransformatorNew) {
				((Phase2TransformatorNew) transformator).fireUnknownEntityDetected(new ExternalEntity(
						qualifiedBaseType, entityEntensions), new ResolutionMarker(Datatype.class, parent));
			}
			// skip e.g. due to LOGGER.unsupported generic type
			return KDMElementFactory.createDatatype("LOGGER.unsupported: " + commonTreeNode.getChildren());
		}
	}

	private Datatype wrapTypeExtensions(final CommonTree commonTreeNode, Datatype type) {
		for (int i = 1; i < commonTreeNode.getChildCount(); i++) {
			Tree typeExtension = commonTreeNode.getChild(i);
			// INTERR, rank_specifier, STAR
			switch (typeExtension.getType()) {
				case CSharp4AST.INTERR:
					LOGGER.warning("UNSUPPORTED: INTERR is not yet supported");
					break;
				case CSharp4AST.RANK_SPECIFIER:
					int numCommas = typeExtension.getChildCount();
					do {
						ItemUnit arrayItemUnit = CodeFactory.eINSTANCE.createItemUnit();
						arrayItemUnit.setName(type.getName());
						arrayItemUnit.setType(type);
						if (type.eContainer() == null) {
							arrayItemUnit.getCodeElement().add(type);
						}

						ArrayType arrayType = CodeFactory.eINSTANCE.createArrayType();
						// arrayType.setIndexUnit(value);
						arrayType.setItemUnit(arrayItemUnit);
						arrayType.setName(arrayItemUnit.getName() + "[]");
						// arrayType.setSize(value);

						type = arrayType;
					} while (numCommas-- > 0);
					break;
				case CSharp4AST.STAR:
					ItemUnit ptrItemUnit = CodeFactory.eINSTANCE.createItemUnit();
					ptrItemUnit.setName(type.getName());
					ptrItemUnit.setType(type);
					if (type.eContainer() == null) {
						ptrItemUnit.getCodeElement().add(type);
					}

					PointerType pointerType = CodeFactory.eINSTANCE.createPointerType();
					pointerType.setItemUnit(ptrItemUnit);
					pointerType.setName(ptrItemUnit.getName() + "*");

					type = pointerType;
					break;
				default:
					break;
			}
		}
		return type;
	}

	private void addGenericToParent(final CodeItem parent, final Datatype child) {
		if (parent instanceof ClassUnit) { // child: method, member, inner type
			((ClassUnit) parent).getCodeElement().add(child);
		} else if (parent instanceof InterfaceUnit) { // child: method, inner
			// type(?)
			((InterfaceUnit) parent).getCodeElement().add(child);
		} else if (parent instanceof CompositeType) {
			((CompositeType) parent).getItemUnit().add((ItemUnit) child);
		} else if (parent instanceof MethodUnit) {
			((MethodUnit) parent).getCodeElement().add(child);
		} else {
			System.err.println("UNSUPPORTED TypeTransformator.addToParent() -> parent: " + parent);
			Pause.pause();
		}
	}

}
