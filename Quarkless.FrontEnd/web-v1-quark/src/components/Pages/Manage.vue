<template>
<div class="ccontainer">
        <div :class="!isNavOn?'navbar-push':''">
                <div class="accounts_container" >
                        <div v-for="(acc,index) in InstagramAccounts" :key="index">
                                <InstaCard @onChangeBiography="onChangeBiography" @onChangeProfilePicture="onChangeProfilePic" 
                                @ChangeState="StateChanged" @RefreshState="NotifyRefresh" @ViewLibrary="GetLibrary" 
                                @ViewProfile="GetProfile" @DeleteAccount="DeleteInstagramAccount" @HandleVerify="onHandleVerify"
                                @HandleProxy="onHandleProxy"
                                :id="acc.id" :username="acc.username" :agentState="acc.agentState" 
                                :name="acc.fullName" :profilePicture="acc.profilePicture" :biography="acc.userBiography"
                                :userFollowers="acc.followersCount" :userFollowing="acc.followingCount" :totalPost="acc.totalPostsCount" :IsProfileButtonDisabled="IsProfileButtonDisabled"/>
                        </div>
                        <div class="card is-hover">
                                <div class="card-content">
                                        <a @click="isAccountLinkModalOpened = true"><h3 class="title is-1">+</h3></a>
                                </div>
                        </div>
                </div>
        </div>
        <b-modal :active.sync="isAccountLinkModalOpened" has-modal-card>
                <div class="modal-card is-custom">
                    <header class="modal-card-head">
                        <p class="modal-card-title">Link your Instagram Account</p>
                    </header>
                    <section class="modal-card-body">
                        <b-field label="Username" custom-class="has-text-white">
                            <b-input
                                type="text"
                                v-model="linkData.username"
                                placeholder="Your Username"
                                required>
                            </b-input>
                        </b-field>
                        <b-field label="Password" custom-class="has-text-white">
                            <b-input
                                type="password"
                                v-model="linkData.password"
                                password-reveal
                                placeholder="Your password"
                                required>
                            </b-input>
                        </b-field>
                        <br>
                        <b-field>
                                <p class="control">
                                        <b-radio-button v-model="linkData.useMyLocation" @input="askForLocation"
                                                native-value="true"
                                                type="is-success">
                                                <b-icon icon="check"></b-icon>
                                                <span>Use My Location</span>
                                        </b-radio-button>
                                </p>
                                <p class="control">
                                        <b-radio-button v-model="linkData.useMyLocation"
                                                native-value="false"
                                                type="is-danger">
                                                <b-icon icon="close"></b-icon>
                                                <span>Manually enter location</span>
                                        </b-radio-button>
                                </p>
                                <p class="control">
                                        <b-radio-button v-model="linkData.useMyLocation"
                                                native-value="proxy"
                                                type="is-warning">
                                                <b-icon icon="pen"></b-icon>
                                                <span>Use My Own Proxy</span>
                                        </b-radio-button>
                                </p>
                        </b-field>
                        <b-field v-if="linkData.useMyLocation === 'proxy'">
                                 <p class="control">
                                        <b-radio-button v-model="linkData.proxy.proxyType" @input="askForLocation"
                                                :native-value="0"
                                                type="is-info">
                                                <b-icon pack="fas" icon="chess"></b-icon>
                                                <span>Http Proxy</span>
                                        </b-radio-button>
                                </p>
                                <p class="control">
                                        <b-radio-button v-model="linkData.proxy.proxyType"
                                                :native-value="1"
                                                type="is-info">
                                                <b-icon pack="fas" icon="chess-knight"></b-icon>
                                                <span>SOCKS5</span>
                                        </b-radio-button>
                                </p>
                                <b-input v-model="linkData.proxy.hostAddress" placeholder="Proxy Address"></b-input>
                                <b-input v-model="linkData.proxy.port" placeholder="Proxy Port"></b-input>
                                <b-input v-model="linkData.proxy.username" placeholder="Proxy Username"></b-input>
                                <b-input v-model="linkData.proxy.password" placeholder="Proxy Password"></b-input>
                        </b-field>
                        <b-field v-if="linkData.useMyLocation === 'false'">
                                <b-autocomplete
                                        size="is-medium"
                                        type="is-dark"
                                        v-model="linkData.location.address"
                                        :data="searchItems"
                                        autocomplete
                                        :allow-new="false"
                                        field="address"
                                        icon="map-marker"
                                        placeholder="Add a location"
                                        @typing="performAutoCompletePlacesSearch">
                                </b-autocomplete>
                        </b-field>
                        <b-message title="Account Linking" type="is-danger" has-icon aria-close-label="Close message">
                        Your Instagram Credentials are encrypted and not seen by anyone
                        </b-message>
                        <b-message v-if="linkData.useMyLocation !== 'proxy'" title="For best experience" type="is-info" has-icon aria-close-label="Close message">
                        Please ensure your account is at least 2 weeks old, Instagram spam detection system targets new accounts more than older ones.
                        </b-message>
                        <b-message v-if="linkData.useMyLocation === 'proxy'" title="Just Letting you know..." type="is-warning" has-icon aria-close-label="Close message">
                        Please Avoid using public proxies as they have most likely been used before and therefore blacklisted by instagram.
                        </b-message>
                    </section>
                    <footer class="modal-card-foot">
                        <button @click="LinkAccount" :class="isLinkingAccount?'button is-light is-rounded is-large is-loading' : 'button is-light is-rounded'" style="margin:0 auto;">
                                <b-icon icon="link">

                                </b-icon>
                                <span>Link Account</span>
                        </button>                    
                    </footer>
                </div>
        </b-modal>
        <div v-if="needToVerify">
               <InstaVerify @Finished="needToVerify = false" :details="verifyDetails"/>
        </div>
</div>
</template>

<script>
import Vue from 'vue';
import InstaCard from "../Objects/InstaAccountCard";
import debounce from 'lodash/debounce'
import InstaVerify from '../Objects/VerifyInstagram';
import {GetUserLocation} from '../../localHelpers'
export default {
        name:"manage",
        components:{
                InstaCard,
                InstaVerify
        },
        data(){
        return{
                searchItems:[],
                IsProfileButtonDisabled:true,
                InstagramAccounts:[],
                alert_text:'',
                isNavOn:false,
                isAccountLinkModalOpened:false,
                isLinkingAccount:false,
                linkData:{
                        proxy:{
                              username:'',
                              password:'',
                              proxyType:0,
                              hostAddress:'',
                              port:'',
                        },
                        username:'',
                        password:'',
                        useMyLocation:'true',
                        location:{
                                address:'',
                                coordinates:{
                                        latitude:0,
                                        longitude:0
                                }
                        }
                },
                verifyDetails:{
                        instagramAccountId:'',
                        challengeDetail:{}
                },
                needToVerify:false,
        }
        },
        created(){
               this.$emit('unSelectAccount');
        },
        mounted(){
		this.isNavOn = this.$store.getters.MenuState === 'true';
                this.InstagramAccounts = this.$store.getters.GetInstagramAccounts;
                if(this.linkData.useMyLocation === 'true'){
                        this.askForLocation();
                }
                if(this.linkData.useMyLocation === 'proxy'){
                        if(!this.linkData.proxy.hostAddress)
                        {
                                return;
                        }
                        if(!this.linkData.proxy.port)
                        {
                                return;
                        }
                }
		if(this.$store.getters.UserProfiles!==undefined)
			this.IsProfileButtonDisabled=false;       
			this.$bus.$on('onFocusBio', (id)=>{
				this.$bus.$emit('cancel-other-focused');
				this.$bus.$emit('focus-main', id);
			})
        },
        computed:{

        },
        methods:{
                askForLocation(){
                        GetUserLocation().then().catch(err=>{
                                // Vue.prototype.$toast.open({
                                //         message: "Oops, looks like you've turned off location sharing for our site, please enable it in order to use this feature",
                                //         type: 'is-info',
                                //         position:'is-bottom',
                                //         duration:8000
                                // })
                                this.linkData.useMyLocation = 'false'
                        })
                },
                performAutoCompletePlacesSearch: debounce(function (query){
                        if(query && query!==''){
                                this.$store.dispatch('GooglePlacesAutoCompleteSearch', {query: query, radius:1500}).then(({ data })=>{
                                this.searchItems = []
                                JSON.parse(data).predictions.forEach((item) =>
                                this.searchItems.push(
                                        {
                                                city:item.structured_formatting.main_text, 
                                                address:item.description
                                        })); // this.searchItems.push(item))
                                })
                        }
                },500),
                clickOutside(){
                        this.$bus.$emit('clickedOutside')
                },
                onHandleProxy(id){
                        let profile = this.$store.getters.UserProfile(id)._id;
                         this.$store.dispatch('SaveProfileStepSection', 
                        {
                                step: 4,
                                profile: profile
                        })
                        this.$router.push('/profile/'+ profile)
                },
                onHandleVerify(id){
                        this.verifyDetails.instagramAccountId = id
                        this.verifyDetails.challengeDetail = this.InstagramAccounts.find(s=>s.id === id).challengeInfo
                        this.needToVerify = true;
                },
                onChangeBiography(data){
                        this.$store.dispatch('ChangeBiography', {instagramAccountId: data.id, biography: data.biography}).then(resp=>{
                                  Vue.prototype.$toast.open({
                                        message: "Successfully Changed Profile Biography",
                                        type: 'is-success',
                                        position:'is-top',
                                        duration:4000
                                });
                                this.$bus.$emit("doneUpdatingBiography")
                        }).catch(err=>{
                                Vue.prototype.$toast.open({
                                        message: "Could not Update your biography at this time",
                                        type: 'is-danger',
                                        position:'is-top',
                                        duration:4000
                                })
                                 this.$bus.$emit("doneUpdatingBiography")
                        })
                },
                onChangeProfilePic(data){
                        this.$store.dispatch('ChangeProfilePicture', {instagramAccountId: data.id, image: data.image}).then(resp=>{
                                 Vue.prototype.$toast.open({
                                        message: "Successfully Changed Profile Picture",
                                        type: 'is-success',
                                        position:'is-top',
                                        duration:4000
                                });
                        }).catch(err=>{
                                Vue.prototype.$toast.open({
                                        message: "Could not Change profile picture at this time",
                                        type: 'is-danger',
                                        position:'is-top',
                                        duration:4000
                                })
                        })
                },
                LinkAccount(){
                        if(this.linkData.username && this.linkData.password){
                                this.isLinkingAccount = true;
                                if(this.linkData.useMyLocation === 'true'){
                                        this.$store.getters.UserInformation.then(res=>{
                                                const locationDetails = res.userInformation.geoLocationDetails
                                                this.linkData.location.address = locationDetails.city + "," + locationDetails.country;
                                                this.linkData.location.coordinates.latitude = locationDetails.location.latitude
                                                this.linkData.location.coordinates.longitude = locationDetails.location.longitude
                                        }).catch(err=>{
                                                this.isLinkingAccount = false;
                                                Vue.prototype.$toast.open({
                                                        message: "Please enter your location manually as we are having trouble detecting your location",
                                                        type: 'is-info',
                                                        position:'is-top',
                                                        duration:4000
                                                })
                                                this.linkData.useMyLocation = 'false'
                                        })
                                }
                                if(this.linkData.useMyLocation === 'proxy'){
                                        if(!this.linkData.proxy.hostAddress){
                                                this.isLinkingAccount = false;
                                                Vue.prototype.$toast.open({
                                                        message: "Please enter the proxy address",
                                                        type: 'is-danger',
                                                        position:'is-bottom',
                                                        duration:8000
                                                })  
                                                return;
                                        }
                                }
                                if(!this.linkData.location.address && this.linkData.useMyLocation !== 'proxy'){
                                        this.isLinkingAccount = false;
                                        return;
                                }
                                       
                                let data = 
                                        {
                                                username:this.linkData.username, 
                                                password:this.linkData.password, 
                                                type:0,
                                                location: this.linkData.location,
                                                enableAutoLocate: this.linkData.useMyLocation === 'true',
                                                proxyDetail : this.linkData.useMyLocation === 'proxy' ? this.linkData.proxy : null
                                        };
                                
                                
                                this.$store.dispatch('LinkInstagramAccount',data).then(resp=>{
                                        const instaId = resp.data.results.instagramAccountId
                                        if(resp.data !== undefined || resp.data !==null || instaId!==undefined){
                                                Vue.prototype.$toast.open({
                                                        message: "Successfully added, will now redirect you to your profile page",
                                                        type: 'is-success',
                                                        position:'is-bottom',
                                                        duration:8000
                                                })

                                                this.$store.dispatch('AccountDetails', {"userId":this.$store.state.user}).then(_=>{

                                                }).catch(err=>{})

                                                this.$store.dispatch('GetProfiles', this.$store.state.user).then(resp=>{
                                                        const index = resp.data.findIndex(ob=>ob.instagramAccountId === instaId)
                                                        if(index > -1){
                                                                const profileId = resp.data[index]._id
                                                                this.$router.push('/profile/'+ profileId)
                                                        }
                                                }).catch(err=>console.log(err.response));
                                        }
                                        this.isLinkingAccount = false;
                                }).catch(err=>{      
                                        Vue.prototype.$toast.open({
                                                message: 'Failed to add account, please ensure the proxy and instagram account details are correct',
                                                type: 'is-danger',
                                                position:'is-bottom',
                                                duration:8000
                                        })
                                
                                        this.isLinkingAccount = false;
                                })
                        }
                },
                GetProfile(id){
                        if(!this.IsProfileButtonDisabled){
                                var profile = this.$store.getters.UserProfiles[this.$store.getters.UserProfiles.findIndex((_)=>_.instagramAccountId == id)];
                                this.$router.push('/profile/'+ profile._id)
                        }
                },
                DeleteInstagramAccount(id){
                        if(id){
                                this.$store.dispatch('DeleteInstagramAccount', id).then(resp=>{
                                        this.InstagramAccounts = this.$store.getters.GetInstagramAccounts;
                                }).catch(err => console.log(err))
                        }
                },
                GetLibrary(id){
                        this.$router.push('/library/'+ id)
                },
                StateChanged(data){
                        this.$store.dispatch('ChangeState', data).then(res=>{
                                if(res){
                                        Vue.prototype.$toast.open({
                                                message: 'Updated!',
                                                type: 'is-success',
                                                position: 'is-bottom',
                                        })
                                window.location.reload();
                                }
                        })
                },
                NotifyRefresh(isSuccess){
                        if(isSuccess){
                                Vue.prototype.$toast.open({
                                        message: 'Account state has been refreshed',
                                        type: 'is-success'
                                })
                        }
                        else{
                                Vue.prototype.$toast.open({
                                        message: 'Could not log into the account',
                                        type: 'is-danger'
                                })
                                window.location.reload();
                        }
                }
        }
}
</script>

<style lang="scss">
@import '../../Style/darkTheme.scss';
.navbar-push{
        margin-left:7em;
}
.card {
        &.is-hover{
                margin-left:0.4em;
                margin-top:1em;
                width:400px !important;
                height:395px !important;
                background-color: #292929 !important;
                color:white !important;
                .card-content{
                        h3{
                                color:wheat;
                                padding:0em;
                                font-size:250px;
                                text-align: center;
                                &:hover{
                                        color:#292929;
                                }
                        }
                        
                }
                &:hover{
                        background:wheat !important;
                }
        }
       
}

.modal-card{
        &.is-custom{
                width: 100%;
                height:45vw;
                padding:0;
                 background-color:$modal_background;
                 .modal-card-body{
                        padding-top:1em;
                        padding-left:2em;
                        padding-right:2em;
                        background:$modal_body;
                        color:$main_font_color;
                        label{
                                //color:$main_font_color;
                                text-align: left;
                        }
                        .input{
                                color:$main_font_color !important;
                                &::placeholder{
                                        color:$main_font_color;
                                }
                                border:none;
                        }
                        .control-label{
                                &:hover{
                                        color:$wheat;
                                }
                        }
                        .dropdown-menu{
                                background:$backround_back;
                                .dropdown-item{
                                        color:$main_font_color;
                                        &:hover{
                                                background:#292929;
                                        }
                                }
                        }
                }
                .modal-card-foot{
                        background:$backround_back;
                        border:none;
                }
                .modal-card-head{
                        background:$backround_back;
                        border:none;
                        .modal-card-title{
                                color:$main_font_color;
                        }
                        
                }
        }
}
.accounts_container{
        width: 100%;
        padding-top:0.5em;
        padding-left:3.5em;
        display: flex;
        flex-flow: row wrap;
        align-items: center;
}

body {
       // overflow-y:hidden;
        width: 100%;
        text-align: center;
}
@media (max-width: 850px) {
        .modal-card{
                height:100vh;
                &.is-custom{
                        height:100vh;
                }
        }
        .navbar-push{
                margin-left: 0em;
        }
        .card {
                &.is-hover{
                        margin-left:0;
                        width:80vw !important;
                }
        }
}
</style>
