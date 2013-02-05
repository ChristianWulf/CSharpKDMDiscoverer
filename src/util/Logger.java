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

import org.apache.logging.log4j.LogManager;

/**
 * @author chw
 */
public class Logger {

	private final org.apache.logging.log4j.Logger	logger;

	public Logger(final Class<?> clazz) {
		logger = LogManager.getLogger(clazz);
	}

	public void unsupported(final Object arg0) {
		logger.warn("UNSUPPORTED: " + arg0);
	}

	public void info(final Object arg0) {
		logger.info(arg0);
	}

	public void warning(final Object arg0) {
		logger.warn(arg0);
	}

	public void fine(final Object arg0) {
		logger.debug(arg0);
	}

	public void fatal(final Object arg0) {
		logger.fatal(arg0);
	}

	public void skip(final Object arg0) {
		logger.warn("Skipped (unsupported): " + arg0);
	}

	public void finer(final Object arg0) {
		logger.trace(arg0);
	}

	public void error(final Throwable t) {
		String msg;
		// msg = ListUtil.combine(Arrays.asList(t.getStackTrace()), "\n\t");
		msg = (t.getLocalizedMessage() != null) ? t.getLocalizedMessage() : "No exception message available.";
		logger.error(msg, t);
	}

	public void error(final String msg) {
		logger.error(msg);
	}

	public void error(final String msg, final Throwable t) {
		logger.error(msg, t);
	}

}
