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

import mapping.TestFileMapperTest;
import mapping.code.TestCSharpProjectDetectorTest;
import mapping.code.TestLanguageUnitCacheTest;
import mapping.code.namespace.TestNamespaceStackTest;
import mapping.util.TestKeyStringHelperTest;

import org.junit.runner.RunWith;
import org.junit.runners.Suite;
import org.junit.runners.Suite.SuiteClasses;

import util.TestFileAccessTest;

@RunWith(Suite.class)
@SuiteClasses({
	TestFileMapperTest.class,
	TestCSharpProjectDetectorTest.class,
	TestLanguageUnitCacheTest.class,
	TestNamespaceStackTest.class,
	TestFileAccessTest.class,
	//	TestASTGrammarGeneratorTest.class,
	TestKeyStringHelperTest.class,
})
public class AllTests {
	// entry point for test execution
}
