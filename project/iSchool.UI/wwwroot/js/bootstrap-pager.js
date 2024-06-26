/**
 * 分页组件 
 * fix from https://github.com/bill1012/bootstrap-pager
 *
 */
(function ($, window, document) {
    'use strict';

    var pluginName = 'page';

    $.fn[pluginName] = function (options) {
        var self = $(this);
        if (this == null) return null;

        var data = this.data(pluginName);
        //if (!data) {
        data = new BasePage(this, options);
        self.data(pluginName, data);
        //}
        return data;
    };


    var BasePage = function (element, options) {
        this.loaded = false;
        this.$element = $(element);
        this.options = $.extend(true, {}, this.default, options);
        this.options.maxPage = this.options.maxPage < 4 ? 4 : this.options.maxPage;
        this.init();
    };

    //默认配置
    BasePage.prototype.default = {
        maxPage: 7,  //最大显示页数
        pageSize: 10,   //每页显示item条数
        canSkip: false, //开启跳页
    };

    BasePage.prototype.template = {
        ul: '<ul class="pagination">',
        li_pre: '<li class="paginate_button page-item previous disabled"></li>',
        li_pre_a: '<a style="cursor:pointer;" class="previous page-link" aria-controls="{0}" data-dt-idx="0" tabindex="0">上一页</a>',
        li_page: '<li class="paginate_button page-item"></li>',
        li_page_a: '<a style="cursor:pointer;"  aria-controls="{0}" data-dt-idx="{1}" tabindex="{1}" class="page-link">{1}</a>',
        li_dots: '<li class="paginate_button page-item"></li>',
        li_dots_a: '<a style="cursor:pointer;"  aria-controls="{0}" class="disabled page-link" tabindex="{1}">{2}</a>',
        li_next: '<li class="paginate_button next page-item disabled"></li>',
        li_next_a: '<a style="cursor:pointer;" class="next page-link"  aria-controls="{0}" data-dt-idx="{1}" tabindex="{1}">下一页</a>',
        li_gopage: '<span class="span-skip" style="margin:0 1em;"> 转到第<input type="text" class="form-control" style="width:45px;display:inline-block;"/>页<button type="button" class="btn btn-primary btn-sm btn-skip" style="margin:0 0.5em;">确定</button></span>'
    };


    BasePage.prototype.refresh = function (pageInfo) {
        var maxPage = this.options.maxPage;
        //var theme=this.options.theme;
        var count = pageInfo.count || this.options.count;
        var pageNum = pageInfo.pageNum || 1;
        var pageSize = pageInfo.pageSize || this.options.pageSize;
        this.$element.page({
            maxPage: maxPage,
            pageSize: pageSize,
            pageNum: pageNum,
            count: count,
            //theme:theme
        });
    };
    //分页初始化
    BasePage.prototype.init = function () {
        this.drawPage(this.options.count, this.options.pageNum || 1);
        this.loaded = true;
    };

    BasePage.prototype.drawPage = function (count, pageNum) {
        count = parseInt(count);
        pageNum = parseInt(pageNum);

        this.$element.find("ul").remove();

        var totalPage = Math.ceil(count / this.options.pageSize);//最大页数
        var id = this.$element.attr("id");
        var ul = $(this.template.ul);
        //if(this.options.theme=="big"){
        //    ul.addClass("pager-"+this.options.theme);
        //}
        var li_pre = $(this.template.li_pre.format(id));
        var li_pre_a = $(this.template.li_pre_a.format(id));
        li_pre.append(li_pre_a);

        var li_next = $(this.template.li_next.format(id));
        var li_next_a = $(this.template.li_next_a.format(id, totalPage + 1));
        li_next.append(li_next_a);

        ul.append(li_pre);

        if (totalPage <= this.options.maxPage) {
            for (var i = 1; i <= totalPage; i++) {
                var li_page = $(this.template.li_page);
                var li_page_a = $(this.template.li_page_a.format(id, i));
                li_page.append(li_page_a);
                ul.append(li_page);
            }
        } else {
            if (pageNum < this.options.maxPage - 2) {
                //后省略
                for (var i = 1; i <= this.options.maxPage - 1; i++) {
                    var li_page = $(this.template.li_page);
                    var li_page_a = $(this.template.li_page_a.format(id, i));
                    li_page.append(li_page_a);
                    ul.append(li_page);
                }
                var li_dots = $(this.template.li_dots);
                var li_dots_a = $(this.template.li_dots_a.format(id, totalPage - 1, '...'));
                li_dots.append(li_dots_a);
                ul.append(li_dots);

                var li_page = $(this.template.li_page);
                var li_page_a = $(this.template.li_page_a.format(id, totalPage));
                li_page.append(li_page_a);
                ul.append(li_page);
            } else if (pageNum > totalPage - this.options.maxPage + 2) {
                //前省略
                var li_page = $(this.template.li_page);
                var li_page_a = $(this.template.li_page_a.format(id, 1));
                li_page.append(li_page_a);
                ul.append(li_page);

                var li_dots = $(this.template.li_dots);
                var li_dots_a = $(this.template.li_dots_a.format(id, 2, '...'));
                li_dots.append(li_dots_a);
                ul.append(li_dots);

                for (var i = totalPage - this.options.maxPage + 2; i <= totalPage; i++) {
                    var li_page = $(this.template.li_page);
                    var li_page_a = $(this.template.li_page_a.format(id, i));
                    li_page.append(li_page_a);
                    ul.append(li_page);
                }
            } else {
                //前后都省略，中间当前页
                var li_page = $(this.template.li_page);
                var li_page_a = $(this.template.li_page_a.format(id, 1));
                li_page.append(li_page_a);
                ul.append(li_page);

                var li_dots = $(this.template.li_dots);
                var li_dots_a = $(this.template.li_dots_a.format(id, 2, '...'));
                li_dots.append(li_dots_a);
                ul.append(li_dots);

                for (var i = pageNum - (Math.floor(this.options.maxPage / 2) - (this.options.maxPage % 2 == 0 ? 2 : 1)), j = i + this.options.maxPage - 3; i <= j; i++) {
                    var li_page = $(this.template.li_page);
                    var li_page_a = $(this.template.li_page_a.format(id, i));
                    li_page.append(li_page_a);
                    ul.append(li_page);
                }

                var li_dots = $(this.template.li_dots);
                var li_dots_a = $(this.template.li_dots_a.format(id, totalPage - 1, '...'));
                li_dots.append(li_dots_a);
                ul.append(li_dots);

                var li_page = $(this.template.li_page);
                var li_page_a = $(this.template.li_page_a.format(id, totalPage));
                li_page.append(li_page_a);
                ul.append(li_page);
            }
        }
        ul.append(li_next);

        this.options.canSkip && ul.append(this.template.li_gopage);

        this.$element.append(ul);

        this.pageNum = pageNum;
        this.totalPage = totalPage;
        this.elementId = id;
        this.bindEvent();
        //构造完成，设置属性
        this.setPreviousButton();
        this.setNextButton();
        this.setActiveButton();
        this.options.canSkip && this.setSkipSpan();
    };

    BasePage.prototype.setPreviousButton = function () {
        if (this.pageNum > 1) {
            this.$element.find('li.previous').removeClass("disabled");
        } else {
            this.$element.find('li.previous').addClass("disabled");
        }
    };

    BasePage.prototype.setNextButton = function () {
        if (this.pageNum < this.totalPage) {
            this.$element.find('li.next').removeClass("disabled")
        } else {
            this.$element.find('li.next').addClass("disabled");
        }
    };

    BasePage.prototype.setActiveButton = function () {
        this.$element.find("li").removeClass("active");
        this.$element.find("a[data-dt-idx='" + this.pageNum + "']").parent().not('.disabled').addClass("active");
    };

    BasePage.prototype.setSkipSpan = function () {
        this.$element.find('span.span-skip').attr('data-totalPageCount', this.totalPage)
            .find('input').val(this.pageNum);
    };

    BasePage.prototype.bindEvent = function () {
        var self = this;

        this.$element.find("li a").not(".disabled").not(".next").not(".previous").each(function (index, item) {
            $(item).on('click', function () {
                var pageNum = $(this).text();
                if (pageNum == self.pageNum) return;
                var pageInfo = { pageNum: pageNum, pageSize: self.options.pageSize };
                self.$element.trigger("pageChanged", pageInfo);
            });
        });

        this.$element.find("li a.next").on('click', function () {
            if (self.pageNum == self.totalPage) return;
            var pageInfo = { pageNum: self.pageNum + 1, pageSize: self.options.pageSize };
            self.$element.trigger("pageChanged", pageInfo);
        });

        this.$element.find("li a.previous").on('click', function () {
            if (self.pageNum == 1) return;
            var pageInfo = { pageNum: self.pageNum - 1, pageSize: self.options.pageSize };
            self.$element.trigger("pageChanged", pageInfo);
        });

        this.$element.find('.btn-skip').on('click', function () {
            var i = self.$element.find('span.span-skip input').val();            
            i = !jQuery.isNumeric(i) ? -1 : parseInt(i, 10);
            i = i <= 0 ? -1 : i > self.totalPage ? self.totalPage : i;
            if (i <= 0 || self.pageNum == i) return self.$element.find('span.span-skip input').val(self.pageNum);
            var pageInfo = { pageNum: i, pageSize: self.options.pageSize };
            self.$element.trigger("pageChanged", pageInfo);
        });
    };

    if (!$.isFunction(String.prototype.format)) {
        String.prototype.format = function () {
            if (arguments.length == 0) return this;
            for (var s = this, i = 0; i < arguments.length; i++)
                s = s.replace(new RegExp("\\{" + i + "\\}", "g"), arguments[i]);
            return s;
        };
    }
})(jQuery, window, document);