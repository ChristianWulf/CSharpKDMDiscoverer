/***************************************************************************
 * Copyright 2012 by
 * + Christian-Albrechts-University of Kiel
 * + Department of Computer Science
 * + Software Engineering Group
 * and others.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ***************************************************************************/

package mapping.code.extern.reference;

import java.util.List;

import mapping.KDMElementFactory;
import mapping.code.extern.entity.DatatypeInfo;
import mapping.code.extern.entity.FieldInfo;
import mapping.code.extern.entity.MethodInfo;
import mapping.code.extern.entity.NamespaceInfo;

import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.MemberUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.MethodUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Attribute;

public class ReferenceInfo2CodeItemMapper {

	public Datatype convert(final DatatypeInfo reference) {
		Datatype datatype;

		if (reference.isInterface()) {
			datatype = KDMElementFactory.createInterfaceUnit();
		} else {
			datatype = KDMElementFactory.createClassUnit();
			((ClassUnit) datatype).setIsAbstract(reference.isAbstract());
		}

		datatype.setName(reference.getName());
		List<String> namespacePath = reference.getFullPath().subList(0,
				reference.getFullPath().size() - 1);
		KDMElementFactory.addQualifiedAttributeTo(datatype, namespacePath);

		return datatype;
	}

	public MethodUnit convert(final MethodInfo reference) {
		MethodUnit methodUnit = KDMElementFactory.createMethodUnit();
		// methodUnit.setExport(value);
		// methodUnit.setKind(value);
		methodUnit.setName(reference.getName());
		// reference.getReturnType();
		// methodUnit.setType(value);
		// reference.getParameters(); TODO
		// methodUnit.getCodeElement().addAll(parameters);
		return methodUnit;
	}

	public MemberUnit convert(final FieldInfo reference) {
		MemberUnit memberUnit = KDMElementFactory.createMemberUnit();
		memberUnit.setName(reference.getName());
		memberUnit.setSize(null);
		// reference.getType() TODO
		memberUnit.setType(null);
		return memberUnit;
	}

	public Namespace convert(final NamespaceInfo reference) {
		String prefix = reference.getQualifiedPath();
		if (!prefix.isEmpty())
			prefix += ".";

		Attribute qualifiedNameAttribute = KDMElementFactory
				.createFullyQualifiedNameAttribute(prefix + reference.getName());
		Namespace namespace = KDMElementFactory
				.createNamespaceUnit(qualifiedNameAttribute);
		namespace.setName(reference.getName());
		return namespace;
	}

	public CodeItem convert(final ReferenceInfo referenceInfo) {
		if (referenceInfo instanceof DatatypeInfo) {
			return convert((DatatypeInfo) referenceInfo);
		} else if (referenceInfo instanceof MethodInfo) {
			return convert((MethodInfo) referenceInfo);
		} else if (referenceInfo instanceof FieldInfo) {
			return convert((FieldInfo) referenceInfo);
		} else if (referenceInfo instanceof NamespaceInfo) {
			return convert((NamespaceInfo) referenceInfo);
		}
		throw new IllegalStateException(this.getClass().getName()
				+ ".convert() -> Unsupported subclass: "
				+ referenceInfo.getClass());
	}

}
