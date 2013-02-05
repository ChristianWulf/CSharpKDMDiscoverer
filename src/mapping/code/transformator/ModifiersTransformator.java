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

import java.util.Arrays;
import java.util.List;

import mapping.code.MoDiscoKDM;

import org.antlr.runtime.tree.CommonTree;
import org.antlr.runtime.tree.Tree;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeElement;
import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.DataElement;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.EnumeratedType;
import org.eclipse.gmt.modisco.omg.kdm.code.ExportKind;
import org.eclipse.gmt.modisco.omg.kdm.code.InterfaceUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.MethodKind;
import org.eclipse.gmt.modisco.omg.kdm.code.MethodUnit;

import util.Logger;

/**
 * @author chw
 */
public class ModifiersTransformator {

	private static final Logger			LOG						= new Logger(ModifiersTransformator.class);
	private static final List<String>	ACCESSIBILITY_MODIFIERS	= Arrays.asList("public", "protected",
			"protected internal", "internal protected",
			"internal", "private");

	public void transformModifiers(final Datatype element, final CommonTree modifiers, final String defaultModifierName) {
		boolean explicitAccessibilityModifier = false;
		if (modifiers != null) {
			for (int i = 0; i < modifiers.getChildCount(); i++) {
				Tree modTree = modifiers.getChild(i);
				String modName = modTree.getText();

				if ("abstract".equals(modName)) {
					if (element instanceof ClassUnit) {
						((ClassUnit) element).setIsAbstract(Boolean.TRUE);
					}
				} else if ("static".equals(modName)) {
					// access modifiers are not yet supported by KDM, so use KDM attributes
					MoDiscoKDM.createAndAddAccessModifierToElement(modName, element);
				} else if (ACCESSIBILITY_MODIFIERS.contains(modName)) {
					// access modifiers are not yet supported by KDM, so use KDM attributes
					MoDiscoKDM.createAndAddAccessModifierToElement(modName, element);
					explicitAccessibilityModifier = true;
				} else {
					LOG.unsupported("ACCESSIBILITY_MODIFIER '" + modName + "'");
				}
			}
		}
		if (!explicitAccessibilityModifier)
			MoDiscoKDM.createAndAddAccessModifierToElement(defaultModifierName, element);
	}

	public void transformMethodModifier(final AbstractCodeElement parent, final MethodUnit methodUnit,
			final CommonTree modifiers) {
		// set defaults that will be overwritten if explicitly declared
		methodUnit.setKind(MethodKind.METHOD);
		if (parent instanceof InterfaceUnit || parent instanceof EnumeratedType) {
			methodUnit.setExport(ExportKind.PUBLIC);
		} else {
			methodUnit.setExport(ExportKind.PRIVATE);
		}

		if (modifiers != null) {
			for (int i = 0; i < modifiers.getChildCount(); i++) {
				Tree modTree = modifiers.getChild(i);
				String modName = modTree.getText();

				if (modName.equals("abstract")) {
					methodUnit.setKind(MethodKind.ABSTRACT);
				} else if (modName.equals("virtual")) {
					methodUnit.setKind(MethodKind.VIRTUAL);
				} else {
					setMethodModifier(modName, methodUnit);
				}
			}
		}
	}

	private void setMethodModifier(final String modName, final MethodUnit methodUnit) {
		MoDiscoKDM.addModifierTo(methodUnit, modName);

		if (modName.equals("public")) {
			methodUnit.setExport(ExportKind.PUBLIC);
		} else if (modName.equals("protected")) {
			methodUnit.setExport(ExportKind.PROTECTED);
		} else if (modName.equals("private")) {
			methodUnit.setExport(ExportKind.PRIVATE);
		} else if (modName.equals("sealed")) {
			methodUnit.setExport(ExportKind.FINAL);
		}
		// TODO see kdm spec for semantics of protected
		/*
		 * else if (modName.equals("protected internal")) {
		 * MoDiscoKDM.addModifierTo(methodUnit, "protected internal");
		 * } else if (modName.equals("internal protected")) {
		 * MoDiscoKDM.addModifierTo(methodUnit, "internal protected");
		 * } else if (modName.equals("internal")) {
		 * MoDiscoKDM.addModifierTo(methodUnit, "internal");
		 * } else if (modName.equals("private")) {
		 * methodUnit.setExport(ExportKind.PRIVATE);
		 * MoDiscoKDM.addModifierTo(methodUnit, "private");
		 * } else if (modName.equals("static")) {
		 * MoDiscoKDM.addModifierTo(methodUnit, "static");
		 * }
		 */
	}

	public void transformVariableModifiers(final DataElement variable, final List<String> modifierNames) {
		MoDiscoKDM.setModifiersTo(modifierNames, variable);
	}

}
