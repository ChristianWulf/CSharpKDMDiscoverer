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

package util;

import java.util.AbstractList;
import java.util.Arrays;
import java.util.Collection;
import java.util.Collections;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;

import org.eclipse.gmt.modisco.omg.kdm.core.KDMEntity;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import com.google.common.collect.Constraints;

public class ListUtil {

	@SuppressWarnings("unchecked")
	public static <T> List<T> flatten(final Collection<? extends Collection<T>> list) {
		List<T> retVal = new LinkedList<T>();
		flatten(list, (Collection<Object>) retVal);
		return retVal;
	}

	public static <T> void flatten(final Collection<?> fromTreeList, final Collection<Object> toFlatList) {
		for (Object item : fromTreeList) {
			if (item instanceof List<?>) {
				flatten((List<?>) item, toFlatList);
			} else {
				toFlatList.add(item);
			}
		}
	}

	public static String combine(final Collection<?> elements, final String combiner) {
		if (elements.isEmpty()) return "";
		StringBuilder builder = new StringBuilder(elements.size() * 100);
		Iterator<?> iter = elements.iterator();
		builder.append(iter.next());
		while (iter.hasNext()) {
			builder.append(combiner);
			builder.append(iter.next());
		}
		return builder.toString();
	}

	public static List<String> codeItemsToNames(final List<? extends KDMEntity> items) {
		List<String> names = new LinkedList<String>();
		for (KDMEntity ci : items) {
			names.add(ci.getName());
		}
		return names;
	}

	public static <T> List<T> newNotNullLinkedList() {
		return Constraints.constrainedList(new LinkedList<T>(), Constraints.notNull());
	}

	public static List<String> splitAtDot(final String text) {
		if (text.isEmpty()) return Collections.emptyList();
		return Arrays.asList(text.split("\\."));
	}

	/**
	 * Wrapper pour NodeList
	 * 
	 * @param list
	 * @return
	 */
	public static List<Node> list(final NodeList list) {
		return new AbstractList<Node>() {
			@Override
			public int size() {
				return list.getLength();
			}

			@Override
			public Node get(final int index) {
				Node item = list.item(index);
				if (item == null) throw new IndexOutOfBoundsException();
				return item;
			}
		};
	}
}
