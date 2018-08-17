package org.mydotey.scf.facade;

import org.mydotey.scf.labeled.LabeledConfigurationManager;
import org.mydotey.scf.labeled.LabeledKey;

/**
 * @author koqizhao
 *
 * May 21, 2018
 */
public class LabeledStringProperties extends StringValueProperties<LabeledKey<String>, LabeledConfigurationManager> {

    public LabeledStringProperties(LabeledConfigurationManager manager) {
        super(manager);
    }

}
