# fastUpgradeWAP
**Fast upgrade for Apache and PHP on Windows.**

Do you work on Windows and use manual configured Apache and PHP instances? 

This command line tool helps you automate the upgrade process using the Apache/PHP windows installation binary packages(.zip) as an input and some configuration parameters to point where the current instalations are and various other options.

### Configuration:
Edit and change the parameters from `fastUpgradeWAP.exe.config` accordingly:

- Apache root folder. Mandatory. Trailing "\" is needed.
```
<setting name="APACHE_ROOT" serializeAs="String">
	<value>C:\Path\To\Apache\</value>
</setting>
```

- PHP root folder. Mandatory. Trailing "\" is needed.
```
<setting name="PHP_ROOT" serializeAs="String">
	<value>C:\Path\To\PHP\</value>
</setting>
```

- Optional. Comma separated service names (exact unique names, no spaces) that will be stopped before Apache upgrade. Will be re-started at the end.
```
<setting name="APACHE_RELATED_SERVICES_TO_STOP" serializeAs="String">
	<value>Apache2.4</value>
</setting>
```
- Optional. Comma separated service names (exact unique names, no spaces) that will be stopped before PHP upgrade. Will be re-started at the end.
```
<setting name="PHP_RELATED_SERVICES_TO_STOP" serializeAs="String">
	<value />
</setting>
```

- Optional. Comma separated relative paths(no spaces) inside the current Apache root. Files and folder you would like to keep in the new version (they will be copied). Folders will be renamed on new instalation, if already present (rename to `<name>_orig`)
```
<setting name="APACHE_FILES_TO_COPY" serializeAs="String">
	<value>conf,modules\mod_fcgid.so</value>
</setting>
```

- Optional. Comma separated relative paths(no spaces) inside the current PHP root. Files and folder you would like to keep in the new version (they will be copied). Folders will be renamed on new instalation, if already present (rename to `<name>_orig`)
```
<setting name="PHP_FILES_TO_COPY" serializeAs="String">
	<value>php.ini,tmp_sessions,tmp_uploads,ext\php_redis.dll,ext\php_rrd.dll</value>
</setting>
```

## Usage:

### Apache:
```
fastUpgradeWAP.exe C:\Kits\httpd-2.4.54-win64-VS16.zip
```
NOTE: _file starting with `httpd` decides if it's Apache upgrade_
NOTE: It expects the `.zip` file to have an inner folder `Apache24`.

### PHP:
```
fastUpgradeWAP.exe C:\Kits\php-8.0.21-nts-Win32-vs16-x64.zip
```
NOTE: _file starting with `php` decides if it's PHP upgrade_

## Permissions
**If `APACHE_RELATED_SERVICES_TO_STOP` or `PHP_RELATED_SERVICES_TO_STOP` are used (non empty), elevated permissions are needed for this to work. Run as Administrator**
