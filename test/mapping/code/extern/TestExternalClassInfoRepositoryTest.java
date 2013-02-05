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

package mapping.code.extern;

import static org.junit.Assert.*;
import static util.test.Assert.*;

import java.io.FileNotFoundException;
import java.util.Arrays;
import java.util.List;

import mapping.KDMElementFactory;
import mapping.code.extern.loader.DatatypeInfoCacheLoader;
import mapping.code.extern.loader.DatatypeInfoFileSystemLoader;
import mapping.code.extern.loader.DatatypeInfoLoadException;
import mapping.code.extern.loader.IDatatypeInfoLoader;
import mapping.code.extern.reference.ReferenceInfo;
import mapping.code.extern.reference.ReferenceListMapper;
import mapping.code.namespace.NamespaceSearcher;

import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.InterfaceUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;
import org.junit.BeforeClass;
import org.junit.Test;

public class TestExternalClassInfoRepositoryTest {

	private static Namespace			externalGlobalNamespace;
	private static ReferenceListMapper	referenceListMapper;
	private static IDatatypeInfoLoader	datatypeInfoLoader;

	@BeforeClass
	public static void beforeClass() {
		CodeModel externalCodeModel = KDMElementFactory
				.createGenericCodeModel("External CodeModel");
		externalGlobalNamespace = NamespaceSearcher.getGlobalNamespaceFrom(externalCodeModel);
		NamespaceSearcher searcher = new NamespaceSearcher(null, externalGlobalNamespace);
		referenceListMapper = new ReferenceListMapper(searcher);
		datatypeInfoLoader = new DatatypeInfoCacheLoader(new DatatypeInfoFileSystemLoader());
	}

	@Test
	public void testGetQualifiedClassNameReturnsOwnClassUnit() throws DatatypeInfoLoadException,
	FileNotFoundException {
		String rootDir = "resource/test/testcases/6";
		ExternalDatatypeInfoRepository repository = new ExternalDatatypeInfoRepository(rootDir,
				datatypeInfoLoader);
		List<String> qualifiedClassName = Arrays.asList("HelloWorld");

		List<ReferenceInfo> referenceSequence = repository
				.getReferenceSequenceFromDecompiledSourceFile(qualifiedClassName);
		referenceListMapper.transform(referenceSequence);
		CodeItem codeItem = referenceListMapper.getLastReferenceResult();

		assertEquals("Hello", codeItem.getName());
		ClassUnit classUnit = assertInstanceof(ClassUnit.class, codeItem);
		assertEquals(Boolean.FALSE, classUnit.getIsAbstract());
	}

	@Test
	public void testGetQualifiedClassNameReturnsClassUnit2() throws DatatypeInfoLoadException,
	FileNotFoundException {
		System.out.println("testGetQualifiedClassNameReturnsClassUnit2--------------------------");
		String rootDir = "resource/test/decompiled";
		ExternalDatatypeInfoRepository repository = new ExternalDatatypeInfoRepository(rootDir,
				datatypeInfoLoader);
		List<String> qualifiedClassName = Arrays.asList("System", "ConfigTreeParser");

		List<ReferenceInfo> referenceSequence = repository
				.getReferenceSequenceFromDecompiledSourceFile(qualifiedClassName);
		referenceListMapper.transform(referenceSequence);
		CodeItem codeItem = referenceListMapper.getLastReferenceResult();

		assertEquals("ConfigTreeParser", codeItem.getName());
		ClassUnit classUnit = assertInstanceof(ClassUnit.class, codeItem);
		assertEquals(Boolean.FALSE, classUnit.getIsAbstract());
	}

	@Test
	public void testGetQualifiedClassNameReturnsClassUnit() throws DatatypeInfoLoadException,
	FileNotFoundException {
		String rootDir = "resource/test/decompiled";
		ExternalDatatypeInfoRepository repository = new ExternalDatatypeInfoRepository(rootDir,
				datatypeInfoLoader);
		List<String> qualifiedClassName = Arrays.asList("System", "Console");

		List<ReferenceInfo> referenceSequence = repository
				.getReferenceSequenceFromDecompiledSourceFile(qualifiedClassName);
		referenceListMapper.transform(referenceSequence);
		CodeItem codeItem = referenceListMapper.getLastReferenceResult();

		assertEquals("Console", codeItem.getName());
		ClassUnit classUnit = assertInstanceof(ClassUnit.class, codeItem);
		assertEquals(Boolean.FALSE, classUnit.getIsAbstract());
	}

	@Test
	public void testGetQualifiedClassNameReturnsInterfaceUnit() throws DatatypeInfoLoadException,
	FileNotFoundException {
		String rootDir = "resource/test/decompiled";
		ExternalDatatypeInfoRepository repository = new ExternalDatatypeInfoRepository(rootDir,
				datatypeInfoLoader);
		List<String> qualifiedClassName = Arrays.asList("System", "ICloneable");

		List<ReferenceInfo> referenceSequence = repository
				.getReferenceSequenceFromDecompiledSourceFile(qualifiedClassName);
		referenceListMapper.transform(referenceSequence);
		CodeItem codeItem = referenceListMapper.getLastReferenceResult();

		assertEquals("ICloneable", codeItem.getName());
		assertInstanceof(InterfaceUnit.class, codeItem);
	}

	@Test
	public void testGetQualifiedClassNameReturnsEnum() throws DatatypeInfoLoadException,
	FileNotFoundException {
		String rootDir = "resource/test/decompiled";
		ExternalDatatypeInfoRepository repository = new ExternalDatatypeInfoRepository(rootDir,
				datatypeInfoLoader);
		List<String> qualifiedClassName = Arrays.asList("System", "ConsoleKey");

		List<ReferenceInfo> referenceSequence = repository
				.getReferenceSequenceFromDecompiledSourceFile(qualifiedClassName);
		referenceListMapper.transform(referenceSequence);
		CodeItem codeItem = referenceListMapper.getLastReferenceResult();

		assertEquals("ConsoleKey", codeItem.getName());
		// TODO first, find and map to KDM Enum representation
		// ClassUnit classUnit = assertInstanceof(ClassUnit.class, datatype);
		// assertEquals(Boolean.FALSE, classUnit.getIsAbstract());
	}

	// @Test(expected = IllegalArgumentException.class)
	// 11.12.2011: BOM marker is now supported
	public void testLoadUTF8FileWithBOM() throws Exception {
		String rootDir = "resource/test/decompiled";
		ExternalDatatypeInfoRepository repository = new ExternalDatatypeInfoRepository(rootDir,
				datatypeInfoLoader);
		List<String> qualifiedClassName = Arrays.asList("System", "IComparable");
		repository.getReferenceSequenceFromDecompiledSourceFile(qualifiedClassName);
	}

}
