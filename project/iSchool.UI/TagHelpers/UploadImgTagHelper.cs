using iSchool.Application.ViewModels;
using iSchool.Domain.Enum;
using iSchool.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.UI.TagHelpers
{
    /// <summary>
    /// 自定上传图片的帮助标记
    /// </summary>
    [HtmlTargetElement("UploadImg")]
    public class UploadImgTagHelper : TagHelper
    {
        /// <summary>
        /// 标签的Name 用于创建多维数组
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 提交的URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 图片裁剪宽度
        /// </summary>
        public int ImgWidth { get; set; } = 12;

        /// <summary>
        /// 图片裁剪高度
        /// </summary>
        public int ImgHeight { get; set; } = 12;

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 是否需要纳入统计
        /// </summary>
        public bool IsComoletion { get; set; } = true;

        /// <summary>
        /// 图片数量
        /// </summary>
        public int MaxCount { get; set; } = 99;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "";
            output.Content.AppendHtml(@$"<div class='form-group'><label class='control-label mb-1'>{Title}</label><br />
<div class='row'>
                                    <div  class='col-3 mb-3' @mouseenter='img.ShowDel=true' @mouseLeave='img.ShowDel=false' style='" + $@"display:inline-block;padding:0 15px;'  v-for=""(img,index) in getImgarraybyname('{Name}')"" >
                                            <i v-show='img.ShowDel' class='fa fa-trash-o float-right' style='right:0;' @click=""removeimg(index,'{Name}')""></i>
                                        <img :src='img.Url.CompressUrl' class='rounded' style='height:12rem;width:12rem;' alt='上传图片'>
                                        <input id = 'logo' name='Schoollogo' value=' ' type='hidden' />
  <input type='text' class='form-control  mt-3 imgtitle c_ignore'  v-model='img.Title' placeholder='请输入图片信息，最多可输入14个字符。' maxlength='14'>
                                    </div>
                                    <div class='container crop-avatar col-3  {(IsComoletion ? "vueimgcompletion" : string.Intern(""))}' imagename='{Name}' imgheight='{ImgHeight}' imgwidth='{ImgWidth}' style='display:inline-block;margin:0;'>
                                        <div @click=""showModal('{Name}')"" class='card avatar-view' v-if=""isShowAddImage('{Name}',{MaxCount})"">
                                            <div class='card-body nav flex-column justify-content-center bg-light text-center' style='height:12rem;width:100%;'>
                                                <span class='text-primary'>上传图片</span>
                                            </div>
                                        </div>
                                        <div class='modal fade avatar-modal' aria-hidden='true' aria-labelledby='avatar-modal-label' role='dialog' tabindex='-1' style='display: none;'>
                                            <div class='modal-dialog modal-lg'>
                                                <div class='modal-content'>
                                                    <div class='avatar-form' action='" + Url + $@"' enctype='multipart/form-data' method='post'>
                                                        <div class='modal-header'>
                                                            <h4 class='modal-title'>{Title}上传</h4>
                                                            <button type = 'button' class='close' data-dismiss='modal'>&times;</button>
                                                        </div>
                                                        <div class='modal-body'>
                                                            <div class='avatar-body'>
                                                                <div class='avatar-upload'>
                                                                    <input class='avatar-src' name='avatar_src' type='hidden'>
                                                                    <input class='avatar-data' name='avatar_data' type='hidden'>
                                                                    <label for='avatarInput'>Local upload</label>
                                                                    <input class='avatar-input' id='avatarInput' accept='image/*' name='avatar_file' type='file'>
                                                                </div>
                                                                <div class='row'>
                                                                    <div class='col-md-8'>
                                                                        <div class='avatar-wrapper'></div>
                                                                    </div>
                                                                    <div class='col-md-4 card-body nav flex-column justify-content-center bg-light text-center' style='margin-top:15px;'>
                                                                         <div>
                                                                            <span class='text-body'>预览略缩图</span>
                                                                        </div>
                                                                        <div class='avatar-preview preview-lg m-auto'><img :src='initPreviewUrl' /></div>
                                                                    </div>
                                                                </div>
<div>
  <input type='text' class='form-control mt-3 imgtitle c_ignore' placeholder='请输入图片信息，最多可输入14个字符。' maxlength='14' >
</div>
                                                            </div>
                                                        </div>
                                                        <div class='modal-footer text-center' style='display:block;'>
                                                            <button class='btn btn-secondary avatar-save' type='button' @click=""uplooad('{Name}')"">确认</button>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                                            <button class='btn btn-secondary logo-reset' type='button' @click=""quit('{Name}')"">取消</button>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class='loading' aria-label='Loading' role='img' tabindex='-1'></div>
 </div>
 </div>
                                </div>");
        }
    }
}
