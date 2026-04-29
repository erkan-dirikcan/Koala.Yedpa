# Metronic 7 Template Reference

## Template Paths

### SCSS Files Location
`D:\Template\Metronic-Metronic-v7\theme\html\demo1\src\sass`

### Layout Templates
`D:\Template\metronic-7.2.9-demo1-1689938078`

### Login Template
`D:/Template/Metronic-Metronic-v7/theme/html/demo1/dist/custom/pages/login/login-1.html`

## Project Structure

```
wwwroot/
├── assets/
│   ├── js/
│   │   └── custom/          # Custom JavaScript files
│   ├── css/                 # Compiled CSS files
│   ├── media/               # Images, fonts, icons
│   └── sass/                # SCSS source files (from template)
└── favicon.ico
```

## Metronic 7 Key Classes

### Aside (Sidebar)
- `aside aside-left aside-fixed` - Main sidebar container
- `brand` - Logo section
- `aside-menu-wrapper` - Menu container wrapper
- `aside-menu` - Actual menu
- `menu-nav` - Navigation list
- `menu-item` - Menu item
- `menu-item-active` - Active menu item
- `menu-item-open` - Expanded submenu
- `menu-item-submenu` - Parent with children
- `menu-submenu` - Submenu container
- `menu-toggle` - Toggle link for submenus

### Header
- `header header-fixed` - Fixed header
- `header-mobile` - Mobile header
- `header-toolbar` - Toolbar section

### Content
- `content d-flex flex-column flex-column-fluid` - Main content wrapper
- `content-fluid` - Full width content
- `container-fluid` - Fluid container

### Cards/Portlets
- `card card-custom` - Custom card
- `card-header` - Card header
- `card-body` - Card body
- `card-footer` - Card footer

### Forms
- `form-control` - Input styling
- `form-group` - Form group wrapper
- `validation-message` - Validation error
- `input-group` - Input with prepend/append

### Buttons
- `btn btn-primary` - Primary button
- `btn btn-secondary` - Secondary button
- `btn btn-success` - Success button
- `btn btn-danger` - Danger button
- `btn btn-warning` - Warning button
- `btn btn-info` - Info button
- `btn btn-light` - Light button
- `btn btn-font-weight-bolder` - Bold text

### Tables
- `table table-bordered` - Bordered table
- `table table-striped` - Striped rows
- `table table-hover` - Hover effect

### Utilities
- `d-flex` - Display flex
- `flex-column` - Column direction
- `flex-row-auto` - Auto width row
- `align-items-center` - Align items center
- `justify-content-between` - Space between
- `mt-5`, `mb-5`, `p-5`, `pl-5`, `pr-5` - Spacing utilities

## Icons

### Keenicons (ki)
```html
<i class="ki ki-bold-more-hor"></i>
```

### Flaticon
```html
<i class="flaticon-cogwheel-2"></i>
```

### SVG Icons
```html
<span class="svg-icon svg-icon-primary svg-icon-2x">
    <svg xmlns="http://www.w3.org/2000/svg" ...>
        <!-- SVG content -->
    </svg>
</span>
```
